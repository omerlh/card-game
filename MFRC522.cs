using System;
using System.Collections.Generic;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Gpio;

namespace cards_game
{
    //C# Fork of https://github.com/mxgxw/MFRC522-python
    public class MFRC522
    {
        private readonly SpiChannel _channel;

        #region MFRC Consts
        private const byte NRSTPD = 22;

        private const byte MAX_LEN = 16;

        private const byte PCD_IDLE       = 0x00;
        private const byte PCD_AUTHENT    = 0x0E;
        private const byte PCD_RECEIVE    = 0x08;
        private const byte PCD_TRANSMIT   = 0x04;
        private const byte PCD_TRANSCEIVE = 0x0C;
        private const byte PCD_RESETPHASE = 0x0F;
        private const byte PCD_CALCCRC    = 0x03;

        public const byte PICC_REQIDL    = 0x26;
        private const byte PICC_REQALL    = 0x52;
        private const byte PICC_ANTICOLL  = 0x93;
        private const byte PICC_SElECTTAG = 0x93;
        private const byte PICC_AUTHENT1A = 0x60;
        private const byte PICC_AUTHENT1B = 0x61;
        private const byte PICC_READ      = 0x30;
        private const byte PICC_WRITE     = 0xA0;
        private const byte PICC_DECREMENT = 0xC0;
        private const byte PICC_INCREMENT = 0xC1;
        private const byte PICC_RESTORE   = 0xC2;
        private const byte PICC_TRANSFER  = 0xB0;
        private const byte PICC_HALT      = 0x50;

        public const byte MI_OK       = 0;
        public const byte MI_NOTAGERR = 1;
        public const byte MI_ERR      = 2;

        private const byte Reserved00     = 0x00;
        private const byte CommandReg     = 0x01;
        private const byte CommIEnReg     = 0x02;
        private const byte DivlEnReg      = 0x03;
        private const byte CommIrqReg     = 0x04;
        private const byte DivIrqReg      = 0x05;
        private const byte ErrorReg       = 0x06;
        private const byte Status1Reg     = 0x07;
        private const byte Status2Reg     = 0x08;
        private const byte FIFODataReg    = 0x09;
        private const byte FIFOLevelReg   = 0x0A;
        private const byte WaterLevelReg  = 0x0B;
        private const byte ControlReg     = 0x0C;
        private const byte BitFramingReg  = 0x0D;
        private const byte CollReg        = 0x0E;
        private const byte Reserved01     = 0x0F;

        private const byte Reserved10     = 0x10;
        private const byte ModeReg        = 0x11;
        private const byte TxModeReg      = 0x12;
        private const byte RxModeReg      = 0x13;
        private const byte TxControlReg   = 0x14;
        private const byte TxAutoReg      = 0x15;
        private const byte TxSelReg       = 0x16;
        private const byte RxSelReg       = 0x17;
        private const byte RxThresholdReg = 0x18;
        private const byte DemodReg       = 0x19;
        private const byte Reserved11     = 0x1A;
        private const byte Reserved12     = 0x1B;
        private const byte MifareReg      = 0x1C;
        private const byte Reserved13     = 0x1D;
        private const byte Reserved14     = 0x1E;
        private const byte SerialSpeedReg = 0x1F;

        private const byte Reserved20        = 0x20;  
        private const byte CRCResultRegM     = 0x21;
        private const byte CRCResultRegL     = 0x22;
        private const byte Reserved21        = 0x23;
        private const byte ModWidthReg       = 0x24;
        private const byte Reserved22        = 0x25;
        private const byte RFCfgReg          = 0x26;
        private const byte GsNReg            = 0x27;
        private const byte CWGsPReg          = 0x28;
        private const byte ModGsPReg         = 0x29;
        private const byte TModeReg          = 0x2A;
        private const byte TPrescalerReg     = 0x2B;
        private const byte TReloadRegH       = 0x2C;
        private const byte TReloadRegL       = 0x2D;
        private const byte TCounterValueRegH = 0x2E;
        private const byte TCounterValueRegL = 0x2F;

        private const byte Reserved30      = 0x30;
        private const byte TestSel1Reg     = 0x31;
        private const byte TestSel2Reg     = 0x32;
        private const byte TestPinEnReg    = 0x33;
        private const byte TestPinValueReg = 0x34;
        private const byte TestBusReg      = 0x35;
        private const byte AutoTestReg     = 0x36;
        private const byte VersionReg      = 0x37;
        private const byte AnalogTestReg   = 0x38;
        private const byte TestDAC1Reg     = 0x39;
        private const byte TestDAC2Reg     = 0x3A;
        private const byte TestADCReg      = 0x3B;
        private const byte Reserved31      = 0x3C;
        private const byte Reserved32      = 0x3D;
        private const byte Reserved33      = 0x3E;
        private const byte Reserved34      = 0x3F;

        #endregion

        public MFRC522()
        {
             Pi.Spi.Channel0Frequency = SpiChannel.MinFrequency;   
             _channel = Pi.Spi.Channel0;

            Pi.Gpio.Pin22.PinMode = GpioPinDriveMode.Output;
            Pi.Gpio.Pin22.Write(GpioPinValue.High);
  
            Reset();
            
            Write(TModeReg, 0x8D);
            Write(TPrescalerReg, 0x3E);
            Write(TReloadRegL, 30);
            Write(TReloadRegH, 0);
            Write(TxAutoReg, 0x40);
            Write(ModeReg, 0x3D);
            AntennaOn();
        }

        public (int status, List<byte> backData) Anticoll()
        {
            var serNumCheck = 0;
            
            var serNum = new List<byte>();
        
            Write(BitFramingReg, 0x00);
            
            serNum.Add(PICC_ANTICOLL);
            serNum.Add(0x20);
            
            (var status, var backData,var backBits) = ToCard(PCD_TRANSCEIVE,serNum.ToArray());
            Console.WriteLine($"Anti {status}");
            if(status == MI_OK)
            {
                var i = 0;
                if (backData.Count == 5)
                {
                    while (i<4)
                    {
                        serNumCheck = serNumCheck ^ backData[i];
                        i = i + 1;
                    }

                    if (serNumCheck != backData[i])
                    {
                        Console.WriteLine($"Oh No! {i}, {serNumCheck}, {backData[i]}");
                        status = MI_ERR;
                    }
                }
            }
            else
                status = MI_ERR;
        
            return (status,backData);
        }

        public (int status,int backLen) Request(byte reqMode)
        {
            //status = None
            //backBits = None
            var TagType = new List<byte>();
            
            Write(BitFramingReg, 0x07);
            
            TagType.Add(reqMode);
            (var status,var backData,var backLen) = ToCard(PCD_TRANSCEIVE, TagType.ToArray());
            var data = BitConverter.ToString(backData.ToArray());
            if ((status != MI_OK) | (backLen != 0x10))
            {
                Console.WriteLine($"Request failed: {status} - {backLen} - {data}");
                status = MI_ERR;
            }
            
            return (status,backLen);
        }

        private IEnumerable<byte> CalulateCRC(byte[] pIndata)
        {
            ClearBitMask(DivIrqReg, 0x04);
            SetBitMask(FIFOLevelReg, 0x80);
            var i = 0;
            while (i<pIndata.Length)
            {
                Write(FIFODataReg, pIndata[i]);
                i = i + 1;
            }

            Write(CommandReg, PCD_CALCCRC);
            i = 0xFF;
            while (true)
            {
                var n = Read(DivIrqReg);
                i = i - 1;
                if (!((i != 0) && (n&0x04) == 0))
                    break;
            }
            var pOutData = new List<byte>();
            pOutData.Add(Read(CRCResultRegL));
            pOutData.Add(Read(CRCResultRegM));
            return pOutData;
        }

        private (int status, List<byte> backData,int backLen) ToCard(byte command, byte[] sendData)
        {
            var backData = new List<byte>();
            var backLen = 0;
            var status = MI_ERR;
            var irqEn = 0x00;
            var waitIRq = 0x00;
            var n = 0;
            var i = 0;

            switch(command)
            {
                case PCD_AUTHENT:
                    irqEn = 0x12;
                    waitIRq = 0x10;
                    break;
                case PCD_TRANSCEIVE:
                    irqEn = 0x77;
                    waitIRq = 0x30;
                    break;
                default:
                    throw new Exception();
            }
            
            Write(CommIEnReg, (byte)(irqEn|0x80));
            ClearBitMask(CommIrqReg, 0x80);
            SetBitMask(FIFOLevelReg, 0x80);
            
            Write(CommandReg, PCD_IDLE);  
            
            while(i<sendData.Length)
            {
                Write(FIFODataReg, sendData[i]);
                i = i+1;
            }
            
            Write(CommandReg, command);
            
            if (command == PCD_TRANSCEIVE)
                SetBitMask(BitFramingReg, 0x80);
            
            i = 2000;
            while (true)
            {
                n = Read(CommIrqReg);
                i = i - 1;
                if (!((i!=0) && (~(n&0x01) == 1) && (~(n&waitIRq) == (byte)1)))
                    break;
            }
            
            ClearBitMask(BitFramingReg, 0x80);
        
            if (i != 0)
            {
                if ((Read(ErrorReg) & 0x1B)==0x00)
                {
                    status = MI_OK;

                    if ((n & irqEn & 0x01) == 1)
                    {
                        status = MI_NOTAGERR;
                    }
                
                    if (command == PCD_TRANSCEIVE)
                    {
                        n = Read(FIFOLevelReg);
                        var lastBits = (Read(ControlReg) & 0x07);
                        if (lastBits != 0)
                            backLen = (n-1)*8 + lastBits;
                        else
                            backLen = n*8;
                        
                        if (n == 0)
                            n = 1;
                        if (n > MAX_LEN)
                            n = MAX_LEN;
                    
                        i = 0;
                        while (i<n)
                        {
                            backData.Add(Read(FIFODataReg));
                            i = i + 1;
                        }
                    }
                }
            }
            else
                status = MI_ERR;

            return (status,backData,backLen);
        }

        private void AntennaOff()
        {
            ClearBitMask(TxControlReg, 0x03);
        }

        private void AntennaOn()
        {
            var temp = Read(TxControlReg);
            var res = temp & 0x03;
            Console.WriteLine($"AntenaOn {temp}");
            if((temp & 0x03) == 0)
                SetBitMask(TxControlReg, 0x03);
        }

        private void ClearBitMask(byte reg, byte mask)
        {
            var tmp = Read(reg);
            Write(reg, (byte)(tmp & (~mask)));
        }

        private void SetBitMask(byte reg, byte mask)
        {
            var tmp = Read(reg);
            Write(reg, (byte)(tmp | mask));
        }

        private void Reset()
        {
            Write(CommandReg, PCD_RESETPHASE);
        }

        private void Write(byte address, byte value)
        {
            Console.WriteLine($"Write: {address} - {value}");
            _channel.SendReceive(new byte[] {
                (byte)((address << 1) & 0x7E),
                value
                });
        }

        private byte Read(byte address)
        {
            Console.WriteLine($"Read: {address}");
            var result =  _channel.SendReceive( new byte[] {
                (byte)((address << 1) & 0x7E | 0x80),
                0
                } );

            return result[1];
        }
    }
}