﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Craft.Net.Data;
using Craft.Net.Data.Blocks;
using Craft.Net.Data.Entities;
using Craft.Net.Data.Items;

namespace Craft.Net.Server.Handlers
{
    internal class InteractionHandlers
    {
        public static double MaxDigDistance = 6; // TODO: Move somewhere else

        public static void PlayerDigging(MinecraftClient client, MinecraftServer server, IPacket _packet)
        {
            var packet = (PlayerDiggingPacket)_packet;
            var position = new Vector3(packet.X, packet.Y, packet.Z);
            // TODO: Enforce line-of-sight
            var block = client.World.GetBlock(position);
            short damage;
            switch (packet.Action)
            {
                case PlayerDiggingPacket.PlayerAction.StartedDigging:
                    if (client.Entity.Position.DistanceTo(position) <= MaxDigDistance)
                    {
                        if (client.Entity.Abilities.InstantMine || block.Hardness == 0)
                            block.OnBlockMined(client.World, position, client.Entity);
                        else
                        {
                            client.ExpectedMiningEnd = DateTime.Now.AddMilliseconds(
                                block.GetHarvestTime(client.Entity.SelectedItem.AsItem(),
                                client.World, client.Entity, out damage) - (client.Ping + 100));
                            client.ExpectedBlockToMine = position;
                        }
                    }
                    break;
                case PlayerDiggingPacket.PlayerAction.FinishedDigging:
                    if (client.Entity.Position.DistanceTo(position) <= MaxDigDistance)
                    {
                        if (client.ExpectedMiningEnd > DateTime.Now || client.ExpectedBlockToMine != position)
                            return;
                        block.GetHarvestTime(client.Entity.SelectedItem.AsItem(),
                                client.World, client.Entity, out damage);
                        if (damage != 0)
                        {
                            var slot = client.Entity.Inventory[client.Entity.SelectedSlot];
                            if (!slot.Empty)
                            {
                                if (slot.AsItem() is ToolItem)
                                {
                                    var tool = slot.AsItem() as ToolItem;
                                    bool destroy = tool.Damage(damage);
                                    slot.Metadata = tool.Data;
                                    if (destroy)
                                        client.Entity.SetSlot(client.Entity.SelectedSlot, ItemStack.EmptyStack);
                                    else
                                        client.Entity.SetSlot(client.Entity.SelectedSlot, slot);
                                }
                            }
                        }
                        block.OnBlockMined(client.World, position, client.Entity);
                        client.Entity.FoodExhaustion += 0.025f;
                    }
                    break;
                case PlayerDiggingPacket.PlayerAction.DropItem:
                case PlayerDiggingPacket.PlayerAction.DropStack:
                    var SlotItem = client.Entity.Inventory[client.Entity.SelectedSlot];
                    if (!SlotItem.Empty)
                    {
                        var ItemCopy = (ItemStack)SlotItem.Clone();
                        if (packet.Action == PlayerDiggingPacket.PlayerAction.DropStack)
                            client.Entity.SetSlot(client.Entity.SelectedSlot, ItemStack.EmptyStack);
                        else
                        {
                            ItemCopy.Count = 1;
                            SlotItem.Count--; // Decrease the player's item by 1
                            if (SlotItem.Count == 0)
                                client.Entity.SetSlot(client.Entity.SelectedSlot, ItemStack.EmptyStack);
                            else
                                client.Entity.SetSlot(client.Entity.SelectedSlot, SlotItem);
                        }
                        var entity = new ItemEntity(client.Entity.GivenPosition + 
                            new Vector3(0, client.Entity.Size.Height, 0), ItemCopy);
                        entity.Velocity = MathHelper.FowardVector(client.Entity) * new Vector3(0.25);
                        server.EntityManager.SpawnEntity(client.World, entity);
                    }
                    break;
            }
        }

        public static void RightClick(MinecraftClient client, MinecraftServer server, IPacket _packet)
        {
            var packet = (RightClickPacket)_packet;
            var slot = client.Entity.Inventory[client.Entity.SelectedSlot];
            var position = new Vector3(packet.X, packet.Y, packet.Z);
            var cursorPosition = new Vector3(packet.CursorX, packet.CursorY, packet.CursorZ);
            Block block = null;
            if (position != -Vector3.One)
            {
                if (position.DistanceTo(client.Entity.Position) > client.Reach)
                    return;
                block = client.World.GetBlock(position);
            }
            bool use = true;
            if (block != null)
                use = block.OnBlockRightClicked(position, AdjustByDirection(packet.Direction), cursorPosition, client.World, client.Entity);
            var item = slot.AsItem();
            if (use && item != null)
            {
                if (block != null)
                {
                    item.OnItemUsedOnBlock(client.World, position, AdjustByDirection(packet.Direction),
                        cursorPosition, client.Entity);
                    if (item is Block)
                    {
                        client.SendPacket(new NamedSoundEffectPacket((item as Block).PlacementSoundEffect,
                            (int)position.X, (int)position.Y, (int)position.Z, 1, 50));
                    }
                }
                else
                    item.OnItemUsed(client.World, client.Entity);
            }
        }

        public static void UseEntity(MinecraftClient client, MinecraftServer server, IPacket _packet)
        {
            var packet = (UseEntityPacket)_packet;
            var target = server.EntityManager.GetEntity(packet.Target);
            if (target == null ||
                server.EntityManager.GetEntityWorld(target) != server.EntityManager.GetEntityWorld(client.Entity) ||
                target.Position.DistanceTo(client.Entity.Position) > client.Reach)
                return;
            target.UsedByEntity(client.World, packet.LeftClick, client.Entity);
            if (target is LivingEntity)
            {
                // Do damage
                // TODO: Move to Craft.Net.Data?
                if (packet.LeftClick)
                {
                    var livingEntity = target as LivingEntity;
                    if (livingEntity.Invulnerable)
                        return;

                    var item = client.Entity.SelectedItem.AsItem();
                    if (item == null)
                        item = new AirBlock();
                    client.Entity.FoodExhaustion += 0.3f;
                    livingEntity.Damage(item.AttackDamage);
                    // TODO: Knockback enchantment
                    livingEntity.Velocity /*+*/= MathHelper.RotateY(new Vector3(0, 0, client.Entity.IsSprinting ? 10 : 3),
                        MathHelper.DegreesToRadians(client.Entity.Yaw));
                    if (livingEntity is PlayerEntity)
                    {
                        (livingEntity as PlayerEntity).LastDamageType = DamageType.Combat;
                        (livingEntity as PlayerEntity).LastAttackingEntity = client.Entity;
                    }
                    // TODO: Physics
                }
            }
        }

        public static void UpdateSign(MinecraftClient client, MinecraftServer server, IPacket _packet)
        {
            var packet = (UpdateSignPacket)_packet;
            var position = new Vector3(packet.X, packet.Y, packet.Z);
            if (position.DistanceTo(client.Entity.Position) < client.Reach)
            {
                var block = client.World.GetBlock(position);
                if (block is SignBlock)
                {
                    var sign = block as SignBlock;
                    var tile = sign.TileEntity as SignTileEntity;
                    tile.Text1 = packet.Text1;
                    tile.Text2 = packet.Text2;
                    tile.Text3 = packet.Text3;
                    tile.Text4 = packet.Text4;
                    sign.TileEntity = tile;
                    client.World.SetBlock(position, sign);
                }
            }
        }

        private static Vector3 AdjustByDirection(byte direction)
        {
            switch (direction)
            {
                case 0:
                    return Vector3.Down;
                case 1:
                    return Vector3.Up;
                case 2:
                    return Vector3.Backwards;
                case 3:
                    return Vector3.Forwards;
                case 4:
                    return Vector3.Left;
                default:
                    return Vector3.Right;
            }
        }
    }
}
