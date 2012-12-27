---
title: Craft&#46;Net
layout: base
---

Craft.Net is the collective name for several Minecraft-related .NET libraries. You can
pick and choose different libraries to accomplish different tasks. Libraries are included
for things such as Minecraft multiplayer networking and world editing, as well as a
multiplayer client and server. The crown jewel of Craft.Net is the fully functional
Minecraft server, with support for everything from survival mode resource gathering,
multiplayer PvP, entity physics, and more.

Craft.Net currently supports Minecraft 1.4.6 on Windows, Linux and Mac.


## Features

This is not an exhaustive list.

### Craft.Net

* Full support for the 1.4.6 (version 51) Minecraft protocol
* Item stack and entity metadata manipulation
* Cryptography support that provides full interoperability with Java clients and servers

### Craft.Net.Data

* Anvil world manipulation
* All 1.4.6 items and blocks
* Entity management and physics simulation
* Custom terrain generation
* Inventory and window management
* Math helpers for common Minecraft and 3D operations

### Craft.Net.Server

* 1\.4\.6 multiplayer server
* Provides a layer on top of Craft.Net.Data for multiplayer Minecraft
* Fast networking - low CPU and memory usage
* Modular and extensible
* In-game weather management

### Craft.Net.Client

*Craft.Net.Client is the newest addition to Craft.Net, and it is unstable and lacking in features.*

* Connect to and play on 1.4.6 multiplayer servers
* Encrypt/decrypt local lastlogin files (useful for launchers)
* Liase with Minecraft.net for session authentication

In short, Craft.Net is the ideal solution for any Minecraft-related activities on the .NET Framework.

## Usage

Craft.Net is a very large, expansive project. A basic overview will be given here, but you are
encouraged to [visit the wiki](https://github.com/SirCmpwn/Craft.Net/wiki) or the
[website](http://sircmpwn.github.com/Craft.Net) to learn more.

### General networking

Use this code to read the next Minecraft packet from a given stream, and write it to another:

    var packet = PacketReader.ReadPacket(stream);
    // ...
    var output = new MinecraftStream(otherStream);
    packet.WriteTo(output);

There are also various crypto utilities for encrypting a stream with AES/CFB, or creating Minecraft-
style SHA-1 hex digests, or decoding/encoding ASN.1 x509 certificates.

### Servers

To run a Minecraft server, simply use the following code:

    var server = new MinecraftServer(new IPEndPoint(IPAddress.Any, 25565));
    var generator = new FlatlandGenerator();
    // Creates a level in the "world" directory, using the flatland generator
    // You may omit "world" to create a level in memory.
    minecraftServer.AddLevel(new Level(generator, "world"));
    minecraftServer.Start();

You need to create a server on a certain endpoint, then provide it a level to spawn players in, and
then start the server.

### Clients

To connect to a Minecraft server, use this code:

    var session = new Session("PlayerName");
    // Uncomment this code to use the user's saved lastlogin
    //var lastLogin = LastLogin.GetLastLogin();
    //var session = Session.DoLogin(lastLogin.Username, lastLogin.Password);
    var client = new MinecraftClient(session);
    // Connect to the server at 127.0.0.1:25565
    client.Connect(new IPEndPoint(IPAddress.Loopback, 25565));

Create a client with a given session (either offline mode with just a username, or online mode with
a username and password via `Session.DoLogin`. Then specify your endpoint and connect.

[Click here](https://gist.github.com/8377075da938b128bef7) if you want to use something like
"c.nerd.nu:25565" and don't know how.

### Data Manipulation

Want to mess with Minecraft data but don't need networking? Use Craft.Net.Data. Here's an example of
loading up a world and changing a block.

    // Loads the level in the world directory
    var level = new Level("world");
    level.World.SetBlock(new Vector3(5, 10, 15), new DiamondBlock());
    level.Save();

And another example for calculating the time to harvest a block with a given tool:

    var block = new GoldBlock();
    short damage; // The damage the item will sustain from mining this block
    int milliseconds = block.GetHarvestTime(new DiamondPickaxe(), out damage);
    // Use this code if you want to see how long a specific entity will take,
    // with regard to things like being underwater:
    milliseconds = block.GetHarvestTime(new DiamondPickaxe(), world, playerEntity, out damage);

Or maybe you want to spawn a random painting based on the available space in the world (i.e. how
vanilla Minecraft does it):

    // CreateEntity(world, on which block, in which direction);
    var entity = PaintingEntity.CreateEntity(world, new Vector3(1, 2, 3), Vector3.North);
    // Creates a painting entity based on the amount of space available in the specified
    // location, choosing a random one from the list of available paintings that are the
    // required size.
    world.OnSpawnEntity(entity);

As you can likely tell, Craft.Net.Data does a lot. You might want to just browse around and see if
it does what you're looking for.

## Contributing

If you wish to contribute your own code to Craft.Net, please create a fork. You are encouraged to follow
the code standards currently in use, and pull requests that do not will be rejected. You are also
encouraged to make small, focused pull requests, rather than large, sweeping changes. For such changes,
it would be better to create an issue instead. You can
[browse other pull requests](https://github.com/SirCmpwn/Craft.Net/pulls?direction=desc&page=1&sort=created&state=closed)
and see which of them have been accepted for more guidance.

## Getting Help

You can get help by making an [issue on GitHub](https://github.com/SirCmpwn/Craft.Net/issues),
or joining #craft.net on irc.freenode.net.  If you are already knowledgable about using
Craft.Net, consider contributing to the [wiki](https://github.com/SirCmpwn/Craft.Net/wiki) for
the sake of others.

## Dependencies

Craft.Net depends on the following tools and assemblies, which are included in the repository:

* [DotNetZip](http://dotnetzip.codeplex.com/) for compression/decompression with zlib
* [fNbt](https://github.com/fragmer/fNbt) for NBT data manipulation

## Licensing

Craft.Net uses the permissive [MIT license](http://www.opensource.org/licenses/mit-license.php/).

In a nutshell:

* You are not restricted on usage of Craft.Net; commercial, private, etc, all fine.
* The developers are not liable for what you do with it.
* Craft.Net is provided "as is" with no warranty.

[Minecraft](http://minecraft.net/) is not officially affiliated with Craft.Net.