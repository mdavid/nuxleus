﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperWebSocket.WebSocketClient.Protocol
{
    class DraftHybi00DataReader : ReaderBase
    {
        private byte? m_Type;
        private int m_TempLength;
        private int? m_Length;

        private const byte m_ClosingHandshakeType = 0xFF;

        public DraftHybi00DataReader(ReaderBase previousCommandReader)
            : base(previousCommandReader)
        {

        }

        public override WebSocketCommandInfo GetCommandInfo(byte[] readBuffer, int offset, int length, out int left)
        {
            left = 0;

            var skipByteCount = 0;

            if (!m_Type.HasValue)
            {
                byte startByte = readBuffer[offset];
                skipByteCount = 1;
                m_Type = startByte;
            }

            //0xxxxxxx: Collect protocol data by end mark
            if ((m_Type.Value & 0x80) == 0x00)
            {
                byte lookForByte = 0xFF;

                int i;

                for (i = offset + skipByteCount; i < offset + length; i++)
                {
                    if (readBuffer[i] == lookForByte)
                    {
                        left = length - (i - offset + 1);

                        if (BufferSegments.Count <= 0)
                        {
                            var commandInfo = new WebSocketCommandInfo(OpCode.Text.ToString(), Encoding.UTF8.GetString(readBuffer, offset + skipByteCount, i - offset - skipByteCount));
                            Reset(false);
                            return commandInfo;
                        }
                        else
                        {
                            AddArraySegment(readBuffer, offset + skipByteCount, i - offset - skipByteCount);
                            var commandInfo = new WebSocketCommandInfo(OpCode.Text.ToString(), Encoding.UTF8.GetString(BufferSegments.ToArrayData()));
                            Reset(true);
                            return commandInfo;
                        }
                    }
                }

                AddArraySegment(readBuffer, offset + skipByteCount, length - skipByteCount);
                return null;
            }
            else//10000000: Collect protocol data by length
            {
                while (!m_Length.HasValue)
                {
                    if (length <= skipByteCount)
                    {
                        //No data to read
                        return null;
                    }

                    byte lengthByte = readBuffer[skipByteCount];
                    //Closing handshake
                    if (lengthByte == 0x00 && m_Type.Value == m_ClosingHandshakeType)
                    {
                        var commandInfo = new WebSocketCommandInfo(OpCode.Close.ToString());
                        Reset(true);
                        return commandInfo;
                    }

                    int thisLength = (int)(lengthByte & 0x7F);
                    m_TempLength = m_TempLength * 128 + thisLength;
                    skipByteCount++;

                    if ((lengthByte & 0x80) != 0x80)
                    {
                        m_Length = m_TempLength;
                        break;
                    }
                }

                int requiredSize = m_Length.Value - BufferSegments.Count;

                int leftSize = length - skipByteCount;

                if (leftSize < requiredSize)
                {
                    AddArraySegment(readBuffer, skipByteCount, length - skipByteCount);
                    return null;
                }
                else
                {
                    left = leftSize - requiredSize;

                    if (BufferSegments.Count <= 0)
                    {
                        var commandInfo = new WebSocketCommandInfo(OpCode.Text.ToString(), Encoding.UTF8.GetString(readBuffer, offset + skipByteCount, requiredSize));
                        Reset(false);
                        return commandInfo;
                    }
                    else
                    {
                        AddArraySegment(readBuffer, offset + skipByteCount, requiredSize);
                        var commandInfo = new WebSocketCommandInfo(Encoding.UTF8.GetString(BufferSegments.ToArrayData()));
                        Reset(true);
                        return commandInfo;
                    }
                }
            }
        }

        void Reset(bool clearBuffer)
        {
            m_Type = null;
            m_Length = null;
            m_TempLength = 0;

            if (clearBuffer)
                BufferSegments.ClearSegements();
        }
    }
}
