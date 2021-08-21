# CS427_SocketDemo
Server and client builds for seminar projects.
**Setup instructions**
1. Ensure Unity version 2020.3.10f
2. Run both server and client projects in 2 separate Unity editors
3. Run the server, then configure the port and IP address in the Network Manager according to your own (default for local host)
4. Run the client side and connect to the same IP address

**Implementation process**
- Determine the topology (dedicated server, p2p, etc)
- Establish TCP or UDP socket communication between client and server
- Define unique NetworkIdentity for each entity in the game
- Packets data should be sent and read in byte arrays
- Include packet id to determine actions taken
- Include packet length to process data
- Create packet senders and handlers on both client and server
- Process packets and update world as long as clients connect
- Apply sync techniques to prevent lag
- Broadcast (LAN), Port-forwarding, Cloud hosting

Tips: Put both server and client code in Unity to handle game logic more easily.

