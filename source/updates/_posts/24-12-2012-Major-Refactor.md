---
title: Major Refactor
layout: post
author: Drew DeVault
github: SirCmpwn
---

Over the past couple of days, I've been working on a huge refactoring of Craft.Net.Server.
I touched almost everything, but importantly, I refactored the entire networking system
to be closer to that of [SMPRoxy](https://github.com/SirCmpwn/SMProxy.git). The unfortunate
side-effect of this is that a ton of your code might be broken. If you rely heavily on
direct packet control in your software, you'll need to rework all of that code. However,
the advantages are numerous:

* [Craft.Net](https://github.com/SirCmpwn/Craft.Net/tree/master/Craft.Net) is now its own
  library, with just networking code (client and server)
* Performance & stablity is way up
* Memory usage is considerably improved
* It's more organized, and easier to hack with
* Dependency on BouncyCastle is no more

So, let's summarize the changes in more detail.

* Instead of directly writing to sockets, clients work in 
  [several nested streams](https://github.com/SirCmpwn/Craft.Net/blob/master/Craft.Net.Server/MinecraftServer.cs#L444).
* All packet code is kept in [its own project](https://github.com/SirCmpwn/Craft.Net/tree/master/Craft.Net)
  * Includes AesStream, MinecraftStream, BufferedStream for helping connections
  * Includes all crypto code
  * Includes all packets, client and server
  * Includes Slot (now a struct) and MetadataDictionary
* No more little threads popping up all the time - all networking is handled by one thread
* Packet handlers are taken out of the packet code
  * Packet code is now only for reading and writing packets
  * Default handlers are registered [here](https://github.com/SirCmpwn/Craft.Net/blob/master/Craft.Net.Server/Handlers/PacketHandlers.cs)

All things considered, Craft.Net is now far more performant, far more organized, and far more
stable. If you need help learning about the new changes, feel free to [drop me an email](mailto:sir@cmpwn.com).