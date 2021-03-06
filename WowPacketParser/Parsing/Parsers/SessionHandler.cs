using System;
using System.Text;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Store.Objects;
using Guid=WowPacketParser.Misc.Guid;

namespace WowPacketParser.Parsing.Parsers
{
    public static class SessionHandler
    {
        [ThreadStatic]
        public static Guid LoginGuid;

        public static Player LoggedInCharacter;

        [Parser(Opcode.SMSG_AUTH_CHALLENGE, ClientVersionBuild.Zero, ClientVersionBuild.V4_2_2_14545)]
        public static void HandleServerAuthChallenge(Packet packet)
        {
            if (ClientVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                packet.ReadInt32("Shuffle Count");

            packet.ReadInt32("Server Seed");

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                for (var i = 0; i < 8; i++)
                    packet.ReadInt32("Server State", i);
        }

        [Parser(Opcode.SMSG_AUTH_CHALLENGE, ClientVersionBuild.V4_2_2_14545)]
        public static void HandleServerAuthChallenge422(Packet packet)
        {
            packet.ReadInt32("Unk1");
            packet.ReadInt32("Unk2");
            packet.ReadInt32("Unk3");
            packet.ReadInt32("Unk4");
            packet.ReadInt32("Server Seed");
            packet.ReadByte("Unk Byte");
            packet.ReadInt32("Unk5");
            packet.ReadInt32("Unk6");
            packet.ReadInt32("Unk7");
            packet.ReadInt32("Unk8");
        }

        [Parser(Opcode.CMSG_AUTH_SESSION, ClientVersionBuild.Zero, ClientVersionBuild.V4_2_2_14545)]
        public static void HandleAuthSession(Packet packet)
        {
            // Do not overwrite version after Handler was initialized
            packet.ReadEnum<ClientVersionBuild>("Client Build", TypeCode.Int32);

            packet.ReadInt32("Unk Int32 1");
            packet.ReadCString("Account");

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
                packet.ReadInt32("Unk Int32 2");

            packet.ReadUInt32("Client Seed");

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V3_3_5a_12340))
            {
                // Some numbers about selected realm
                packet.ReadInt32("Unk Int32 3");
                packet.ReadInt32("Unk Int32 4");
                packet.ReadInt32("Unk Int32 5");
            }

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V3_2_0_10192))
                packet.ReadInt64("Unk Int64");

            packet.WriteLine("Proof SHA-1 Hash: " + Utilities.ByteArrayToHexString(packet.ReadBytes(20)));

            AddonHandler.ReadClientAddonsList(ref packet);
        }

        //[Parser(Opcode.CMSG_AUTH_SESSION, ClientVersionBuild.V4_2_0_14333)]
        [Parser(Opcode.CMSG_AUTH_SESSION, ClientVersionBuild.V4_2_2_14545, ClientVersionBuild.V4_3_0_15005)]
        public static void HandleAuthSession422(Packet packet)
        {
            packet.ReadByte("Byte");
            packet.ReadByte("Byte");
            packet.ReadInt32("Int32");
            packet.ReadInt32("Int32");
            packet.ReadByte("Byte");
            packet.ReadByte("Byte");
            packet.ReadByte("Byte");

            packet.ReadByte("Byte");
            packet.ReadByte("Byte");
            packet.ReadByte("Byte");
            packet.ReadEnum<ClientVersionBuild>("Client Build", TypeCode.Int16);

            packet.ReadByte("Byte");
            packet.ReadByte("Byte");
            packet.ReadByte("Byte");
            packet.ReadByte("Byte");

            packet.ReadInt32("Int32");
            packet.ReadByte("Byte");
            packet.ReadByte("Byte");
            packet.ReadByte("Byte");
            packet.ReadByte("Byte");

            packet.ReadInt32("Int32");
            packet.ReadByte("Byte");

            packet.ReadInt32("Int32");
            packet.ReadByte("Byte");

            packet.ReadInt32("Int32");
            packet.ReadByte("Byte");

            packet.ReadInt32("Int32");
            packet.ReadInt32("Int32");

            packet.ReadCString("Account name");
            packet.ReadInt32("Int32");

            AddonHandler.ReadClientAddonsList(ref packet);
        }

        [Parser(Opcode.CMSG_AUTH_SESSION, ClientVersionBuild.V4_3_0_15005, ClientVersionBuild.V4_3_2_15211)]
        public static void HandleAuthSession430(Packet packet)
        {
            packet.ReadInt32("Int32");
            packet.ReadByte("Digest (1)");
            packet.ReadInt64("Int64");
            packet.ReadInt32("Int32");
            packet.ReadByte("Digest (2)");
            packet.ReadInt32("Int32");
            packet.ReadByte("Digest (3)");

            packet.ReadInt32("Int32");
            for (var i = 0; i < 7; i++)
                packet.ReadByte("Digest (4)", i);

            packet.ReadEnum<ClientVersionBuild>("Client Build", TypeCode.Int16);

            for (var i = 0; i < 8; i++)
                packet.ReadByte("Digest (5)", i);

            packet.ReadByte("Unk Byte");
            packet.ReadByte("Unk Byte");

            packet.ReadInt32("Client Seed");

            for (var i = 0; i < 2; i++)
                packet.ReadByte("Digest (6)", i);

            using (var pkt = new Packet(packet.ReadBytes(packet.ReadInt32()), packet.Opcode, packet.Time, packet.Direction, packet.Number, packet.Writer, packet.FileName))
            {
                var pkt2 = pkt;
                AddonHandler.ReadClientAddonsList(ref pkt2);
            }
            packet.ReadByte("Mask"); // TODO: Seems to affect how the size is read
            var size = (packet.ReadByte() >> 4);
            packet.WriteLine("Size: " + size);
            packet.WriteLine("Account name: {0}", Encoding.UTF8.GetString(packet.ReadBytes(size)));
        }

        [Parser(Opcode.CMSG_AUTH_SESSION, ClientVersionBuild.V4_3_2_15211, ClientVersionBuild.V4_3_3_15354)]
        public static void HandleAuthSession432(Packet packet)
        {
            var sha = new byte[20];
            packet.ReadInt32("Int32");
            sha[12] = packet.ReadByte();
            packet.ReadInt32("Int32");
            packet.ReadInt32("Int32");
            sha[0] = packet.ReadByte();
            sha[2] = packet.ReadByte();
            sha[18] = packet.ReadByte();
            sha[7] = packet.ReadByte();
            sha[9] = packet.ReadByte();
            sha[19] = packet.ReadByte();
            sha[17] = packet.ReadByte();
            sha[6] = packet.ReadByte();
            sha[11] = packet.ReadByte();

            packet.ReadEnum<ClientVersionBuild>("Client Build", TypeCode.Int16);

            sha[15] = packet.ReadByte();

            packet.ReadInt64("Int64");
            packet.ReadByte("Unk Byte");
            packet.ReadByte("Unk Byte");
            sha[3] = packet.ReadByte();
            sha[10] = packet.ReadByte();

            packet.ReadInt32("Client Seed");

            sha[16] = packet.ReadByte();
            sha[4] = packet.ReadByte();
            packet.ReadInt32("Int32");
            sha[14] = packet.ReadByte();
            sha[8] = packet.ReadByte();
            sha[5] = packet.ReadByte();
            sha[1] = packet.ReadByte();
            sha[13] = packet.ReadByte();

            using (var pkt = new Packet(packet.ReadBytes(packet.ReadInt32()), packet.Opcode, packet.Time, packet.Direction, packet.Number, packet.Writer, packet.FileName))
            {
                var pkt2 = pkt;
                AddonHandler.ReadClientAddonsList(ref pkt2);
            }

            var highBits = packet.ReadByte() << 5;
            var lowBits = packet.ReadByte() >> 3;
            var size = lowBits | highBits;
            packet.WriteLine("Size: " + size);
            packet.WriteLine("Account name: {0}", Encoding.UTF8.GetString(packet.ReadBytes(size)));
            packet.WriteLine("Proof SHA-1 Hash: " + Utilities.ByteArrayToHexString(sha));
        }

        [Parser(Opcode.CMSG_AUTH_SESSION, ClientVersionBuild.V4_3_4_15595)]
        public static void HandleAuthSession434(Packet packet)
        {
            var sha = new byte[20];
            packet.ReadUInt32("UInt32 1");
            packet.ReadUInt32("UInt32 2");
            packet.ReadByte("Unk Byte");
            sha[10] = packet.ReadByte();
            sha[18] = packet.ReadByte();
            sha[12] = packet.ReadByte();
            sha[5] = packet.ReadByte();
            packet.ReadInt64("Int64");
            sha[15] = packet.ReadByte();
            sha[9] = packet.ReadByte();
            sha[19] = packet.ReadByte();
            sha[4] = packet.ReadByte();
            sha[7] = packet.ReadByte();
            sha[16] = packet.ReadByte();
            sha[3] = packet.ReadByte();
            packet.ReadEnum<ClientVersionBuild>("Client Build", TypeCode.Int16);
            sha[8] = packet.ReadByte();
            packet.ReadUInt32("UInt32 3");
            packet.ReadByte("Unk Byte");
            sha[17] = packet.ReadByte();
            sha[6] = packet.ReadByte();
            sha[0] = packet.ReadByte();
            sha[1] = packet.ReadByte();
            sha[11] = packet.ReadByte();
            packet.ReadUInt32("Client seed");
            sha[2] = packet.ReadByte();
            packet.ReadUInt32("UInt32 4");
            sha[14] = packet.ReadByte();
            sha[13] = packet.ReadByte();

            using (var addons = new Packet(packet.ReadBytes(packet.ReadInt32()), packet.Opcode, packet.Time, packet.Direction, packet.Number, packet.Writer, packet.FileName))
            {
                var pkt2 = addons;
                AddonHandler.ReadClientAddonsList(ref pkt2);
            }

            var highBits = packet.ReadByte() << 5;
            var lowBits = packet.ReadByte() >> 3;
            var size = lowBits | highBits;
            packet.WriteLine("Account name: {0}", Encoding.UTF8.GetString(packet.ReadBytes(size)));
            packet.WriteLine("Proof SHA-1 Hash: " + Utilities.ByteArrayToHexString(sha));
        }

        [Parser(Opcode.SMSG_AUTH_RESPONSE, ClientVersionBuild.Zero, ClientVersionBuild.V4_3_4_15595)]
        public static void HandleAuthResponse(Packet packet)
        {
            var code = packet.ReadEnum<ResponseCode>("Auth Code", TypeCode.Byte);

            switch (code)
            {
                case ResponseCode.AUTH_OK:
                {
                    ReadAuthResponseInfo(ref packet);
                    break;
                }
                case ResponseCode.AUTH_WAIT_QUEUE:
                {
                    if (packet.Length <= 6)
                    {
                        ReadQueuePositionInfo(ref packet);
                        break;
                    }

                    ReadAuthResponseInfo(ref packet);
                    ReadQueuePositionInfo(ref packet);
                    break;
                }
            }
        }

        [Parser(Opcode.SMSG_AUTH_RESPONSE, ClientVersionBuild.V4_3_4_15595)]
        public static void HandleAuthResponse434(Packet packet)
        {
            var isQueued = packet.ReadBit();
            var hasAccountInfo = packet.ReadBit();

            if (isQueued)
            {
                var unkByte = packet.ReadByte();
                packet.WriteLine("Unk Byte: " + unkByte);

                var position = packet.ReadInt32();
                packet.WriteLine("Queue Position: " + position);


            }
            if (hasAccountInfo)
            {
                packet.ReadInt32("Billing Time Remaining");
                packet.ReadEnum<ClientType>("Account Expansion", TypeCode.Byte);
                packet.ReadInt32("Unknown UInt32");
                packet.ReadEnum<ClientType>("Player Expansion", TypeCode.Byte);
                packet.ReadInt32("Billing Time Rested");
                packet.ReadEnum<BillingFlag>("Billing Flags", TypeCode.Byte);
            }

            var code = (ResponseCode)packet.ReadByte();
            packet.WriteLine("Auth Code: " + code);

        }

        public static void ReadAuthResponseInfo(ref Packet packet)
        {
            packet.ReadInt32("Billing Time Remaining");
            packet.ReadEnum<BillingFlag>("Billing Flags", TypeCode.Byte);
            packet.ReadInt32("Billing Time Rested");

            // Unknown, these two show the same as expansion payed for.
            // Eg. If account only has payed for Wotlk expansion it will show 2 for both.
            packet.ReadEnum<ClientType>("Account Expansion", TypeCode.Byte);
            if (ClientVersion.AddedInVersion(ClientVersionBuild.V4_0_3_13329))
                packet.ReadEnum<ClientType>("Account Expansion", TypeCode.Byte);
        }

        public static void ReadQueuePositionInfo(ref Packet packet)
        {
            packet.ReadInt32("Queue Position");
            packet.ReadBoolean("Realm Has Free Character Migration");
        }

        [Parser(Opcode.CMSG_PLAYER_LOGIN, ClientVersionBuild.Zero, ClientVersionBuild.V4_2_2_14545)]
        public static void HandlePlayerLogin(Packet packet)
        {
            var guid = packet.ReadGuid("GUID");
            LoginGuid = guid;
        }

        [Parser(Opcode.CMSG_PLAYER_LOGIN, ClientVersionBuild.V4_2_2_14545, ClientVersionBuild.V4_3_0_15005)]
        public static void HandlePlayerLogin422(Packet packet)
        {
            var bits = new bool[8];
            for (var i = 0; i < 8; ++i)
                bits[i] = packet.ReadBit();

            var bytes = new byte[8];
            if (bits[6]) bytes[5] = (byte)(packet.ReadByte() ^ 1);
            if (bits[0]) bytes[0] = (byte)(packet.ReadByte() ^ 1);
            if (bits[4]) bytes[3] = (byte)(packet.ReadByte() ^ 1);
            if (bits[1]) bytes[4] = (byte)(packet.ReadByte() ^ 1);
            if (bits[2]) bytes[7] = (byte)(packet.ReadByte() ^ 1);
            if (bits[5]) bytes[2] = (byte)(packet.ReadByte() ^ 1);
            if (bits[7]) bytes[6] = (byte)(packet.ReadByte() ^ 1);
            if (bits[3]) bytes[1] = (byte)(packet.ReadByte() ^ 1);

            var guid = new Guid(BitConverter.ToUInt64(bytes, 0));
            packet.WriteLine("GUID: {0}", guid);
            LoginGuid = guid;
        }

        [Parser(Opcode.CMSG_PLAYER_LOGIN, ClientVersionBuild.V4_3_0_15005, ClientVersionBuild.V4_3_3_15354)]
        public static void HandlePlayerLogin430(Packet packet)
        {
            var bits = new bool[8];
            for (var i = 0; i < 8; ++i)
                bits[i] = packet.ReadBit();

            var bytes = new byte[8];
            if (bits[3]) bytes[4] = (byte)(packet.ReadByte() ^ 1);
            if (bits[7]) bytes[1] = (byte)(packet.ReadByte() ^ 1);
            if (bits[4]) bytes[7] = (byte)(packet.ReadByte() ^ 1);
            if (bits[6]) bytes[2] = (byte)(packet.ReadByte() ^ 1);
            if (bits[5]) bytes[6] = (byte)(packet.ReadByte() ^ 1);
            if (bits[1]) bytes[5] = (byte)(packet.ReadByte() ^ 1);
            if (bits[2]) bytes[3] = (byte)(packet.ReadByte() ^ 1);
            if (bits[0]) bytes[0] = (byte)(packet.ReadByte() ^ 1);

            var guid = new Guid(BitConverter.ToUInt64(bytes, 0));
            packet.WriteLine("GUID: {0}", guid);
            LoginGuid = guid;
        }

        [Parser(Opcode.CMSG_PLAYER_LOGIN, ClientVersionBuild.V4_3_3_15354, ClientVersionBuild.V4_3_4_15595)]
        public static void HandlePlayerLogin433(Packet packet)
        {
            var bits = new bool[8];
            for (var i = 0; i < 8; ++i)
                bits[i] = packet.ReadBit();

            var bytes = new byte[8];
            if (bits[5]) bytes[1] = (byte)(packet.ReadByte() ^ 1);
            if (bits[2]) bytes[4] = (byte)(packet.ReadByte() ^ 1);
            if (bits[1]) bytes[7] = (byte)(packet.ReadByte() ^ 1);
            if (bits[7]) bytes[2] = (byte)(packet.ReadByte() ^ 1);
            if (bits[6]) bytes[3] = (byte)(packet.ReadByte() ^ 1);
            if (bits[0]) bytes[6] = (byte)(packet.ReadByte() ^ 1);
            if (bits[4]) bytes[0] = (byte)(packet.ReadByte() ^ 1);
            if (bits[3]) bytes[5] = (byte)(packet.ReadByte() ^ 1);

            var guid = new Guid(BitConverter.ToUInt64(bytes, 0));
            packet.WriteLine("GUID: {0}", guid);
            LoginGuid = guid;
        }

        [Parser(Opcode.CMSG_PLAYER_LOGIN, ClientVersionBuild.V4_3_4_15595)]
        public static void HandlePlayerLogin434(Packet packet)
        {
            var bits = new bool[8];
            for (var i = 0; i < 8; ++i)
                bits[i] = packet.ReadBit();

            var bytes = new byte[8];
            if (bits[0]) bytes[2] = (byte)(packet.ReadByte() ^ 1);
            if (bits[7]) bytes[7] = (byte)(packet.ReadByte() ^ 1);
            if (bits[2]) bytes[0] = (byte)(packet.ReadByte() ^ 1);
            if (bits[1]) bytes[3] = (byte)(packet.ReadByte() ^ 1);
            if (bits[5]) bytes[5] = (byte)(packet.ReadByte() ^ 1);
            if (bits[3]) bytes[6] = (byte)(packet.ReadByte() ^ 1);
            if (bits[6]) bytes[1] = (byte)(packet.ReadByte() ^ 1);
            if (bits[4]) bytes[4] = (byte)(packet.ReadByte() ^ 1);

            var guid = new Guid(BitConverter.ToUInt64(bytes, 0));
            packet.WriteLine("GUID: {0}", guid);
            LoginGuid = guid;
        }

        [Parser(Opcode.SMSG_CHARACTER_LOGIN_FAILED)]
        public static void HandleLoginFailed(Packet packet)
        {
            packet.ReadEnum<ResponseCode>("Fail reason", TypeCode.Byte);
        }

        [Parser(Opcode.SMSG_LOGOUT_RESPONSE)]
        public static void HandlePlayerLogoutResponse(Packet packet)
        {
            packet.ReadInt32("Reason");
            packet.ReadBoolean("Instant");
            // From TC:
            // Reason 1: IsInCombat
            // Reason 2: InDuel or frozen by GM
            // Reason 3: Jumping or Falling
        }

        [Parser(Opcode.SMSG_LOGOUT_COMPLETE)]
        public static void HandleLogoutComplete(Packet packet)
        {
            LoggedInCharacter = null;
        }

        [Parser(Opcode.SMSG_REDIRECT_CLIENT, ClientVersionBuild.Zero, ClientVersionBuild.V4_2_2_14545)]
        public static void HandleRedirectClient(Packet packet)
        {
            var ip = packet.ReadIPAddress();
            packet.WriteLine("IP Address: {0}", ip);
            packet.ReadUInt16("Port");
            packet.ReadInt32("Token");
            var hash = packet.ReadBytes(20);
            packet.WriteLine("Address SHA-1 Hash: {0}", Utilities.ByteArrayToHexString(hash));
        }

        [Parser(Opcode.SMSG_REDIRECT_CLIENT, ClientVersionBuild.V4_2_2_14545)]
        public static void HandleRedirectClient422(Packet packet)
        {
            var hash = packet.ReadBytes(255);
            packet.WriteLine("RSA Hash: {0}", Utilities.ByteArrayToHexString(hash));
            packet.ReadInt16("Int 16");
            packet.ReadEnum<UnknownFlags>("Unknown int32 flag", TypeCode.Int32);
            packet.ReadInt64("Int 64");
        }

        [Parser(Opcode.CMSG_REDIRECTION_FAILED)]
        public static void HandleRedirectFailed(Packet packet)
        {
            var token = packet.ReadInt32();
            packet.WriteLine("Token: " + token);
        }

        [Parser(Opcode.CMSG_REDIRECTION_AUTH_PROOF, ClientVersionBuild.Zero, ClientVersionBuild.V4_2_2_14545)]
        public static void HandleRedirectionAuthProof(Packet packet)
        {
            var name = packet.ReadCString();
            packet.WriteLine("Account: " + name);

            var unk = packet.ReadInt64();
            packet.WriteLine("Unk Int64: " + unk);

            var hash = packet.ReadBytes(20);
            packet.WriteLine("Proof SHA-1 Hash: " + Utilities.ByteArrayToHexString(hash));
        }

        [Parser(Opcode.CMSG_REDIRECTION_AUTH_PROOF, ClientVersionBuild.V4_2_2_14545)]
        public static void HandleRedirectionAuthProof422(Packet packet)
        {
            var bytes = new byte[20];
            bytes[0] = packet.ReadByte();
            bytes[12] = packet.ReadByte();
            bytes[3] = packet.ReadByte();
            bytes[17] = packet.ReadByte();
            bytes[11] = packet.ReadByte();
            bytes[13] = packet.ReadByte();
            bytes[5] = packet.ReadByte();
            bytes[9] = packet.ReadByte();
            bytes[6] = packet.ReadByte();
            bytes[19] = packet.ReadByte();
            bytes[15] = packet.ReadByte();
            bytes[18] = packet.ReadByte();
            bytes[8] = packet.ReadByte();
            packet.ReadInt64("Unk long 1");
            bytes[2] = packet.ReadByte();
            bytes[1] = packet.ReadByte();
            packet.ReadInt64("Unk long 2");
            bytes[7] = packet.ReadByte();
            bytes[4] = packet.ReadByte();
            bytes[16] = packet.ReadByte();
            bytes[14] = packet.ReadByte();
            bytes[10] = packet.ReadByte();
            packet.WriteLine("Proof RSA Hash: " + Utilities.ByteArrayToHexString(bytes));
        }

        [Parser(Opcode.SMSG_KICK_REASON)]
        public static void HandleKickReason(Packet packet)
        {
            var reason = (KickReason)packet.ReadByte();
            packet.WriteLine("Reason: " + reason);

            if (!packet.CanRead())
                return;

            var str = packet.ReadCString();
            packet.WriteLine("Unk String: " + str);
        }

        [Parser(Opcode.SMSG_MOTD)]
        public static void HandleMessageOfTheDay(Packet packet)
        {
            var lineCount = packet.ReadInt32();
            packet.WriteLine("Line Count: " + lineCount);

            for (var i = 0; i < lineCount; i++)
            {
                var lineStr = packet.ReadCString();
                packet.WriteLine("Line " + i + ": " + lineStr);
            }
        }
    }
}
