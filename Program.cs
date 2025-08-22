using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrevisFnIoLib;

namespace IOControl
{
    class Program
    {
        static public CrevisFnIO m_cFnIo = new CrevisFnIO();
        static public Int32 m_Err = 0;
        static public IntPtr m_hSystem = new IntPtr();
        static public IntPtr m_hDevivce = new IntPtr();


        static int IoDataControls(IntPtr hDevice)
        {
            byte[] pInputImage = null;
            byte[] pOutputImage = null;

            int val = 0;
            int InputImageSize = 0;
            int OutputImageSize = 0;

            byte OutputVal = 0;
            int i = 0;
    
            Console.Write("****** IO data controls ******\n");

            //
            // Get Input Image Size & Allocate Memory
            //
            m_Err = m_cFnIo.FNIO_DevGetParam(hDevice, CrevisFnIO.DEV_INPUT_IMAGE_SIZE, ref val);
            if (m_Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
                return m_Err;

            InputImageSize = val;
            Console.Write("Input Image Size : {0}\n", InputImageSize);
            pInputImage = new byte[InputImageSize];


            //
            // Get Output Image Size & Allocate Memory
            //
            m_Err = m_cFnIo.FNIO_DevGetParam(hDevice, CrevisFnIO.DEV_OUTPUT_IMAGE_SIZE, ref val);
            if (m_Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
                return m_Err;

            OutputImageSize = val;
            Console.Write("Output Image Size : {0}\n", OutputImageSize);
            pOutputImage = new byte[OutputImageSize];

            // ******************************************************
            // Set the environment variable IO data controls

            //
            // Set the IO data update frequency to maximum speed.
            //
            val = 0;	//0 ms 
            m_Err = m_cFnIo.FNIO_DevSetParam(hDevice, CrevisFnIO.DEV_UPDATE_FREQUENCY, val);
            if (m_Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
                return m_Err;

            //
            // Set the response timeout to 1 second. 
            //
            val = 1000;	//1 s 
            m_Err = m_cFnIo.FNIO_DevSetParam(hDevice, CrevisFnIO.DEV_RESPONSE_TIMEOUT, val);
            if (m_Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
                return m_Err;
            
            // *****************************************************




            Console.Write("Press any key to srart and stop.\n\n");

            Console.ReadKey(true);

            //
            // IO data update start
            //
            m_Err = m_cFnIo.FNIO_DevIoUpdateStart(hDevice, CrevisFnIO.IO_UPDATE_PERIODIC);
            if (m_Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
                return m_Err;


            OutputVal = 0;
            
            while(true)
            {
                OutputVal = (byte)(OutputVal % 255);

                //
                // Write output image data
                //
                if (OutputImageSize > 0)
                {
                    for (i = 0; i < OutputImageSize; i++)
                        pOutputImage[i] = OutputVal;            //Set Output Data


                    m_Err = m_cFnIo.FNIO_DevWriteOutputImage(hDevice, 0, ref pOutputImage[0], OutputImageSize);
                    if (m_Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
                        return m_Err;

                    Console.Write("Write output image data : ");

                    foreach (byte OutputData in pOutputImage)
                    {
                        Console.Write("{0:X2} ", OutputData);
                    }

                    Console.Write("\n");

                }
                else
                {
                    Console.Write("Device has not output image.");
                }

                System.Threading.Thread.Sleep(100);


                //
                // Read input image data
                //
                if (InputImageSize > 0)
                {

                    m_Err = m_cFnIo.FNIO_DevReadInputImage(hDevice, 0, ref pInputImage[0], InputImageSize);
                    if (m_Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
                        return m_Err;

                    Console.Write("Read input image data : ");

                    foreach (byte inputData in pInputImage)
                    {
                        Console.Write("{0:X2} ", inputData);
                    }
                    
                    Console.Write("\n");

                }
                else
                {
                    Console.Write("Device has not input image.");
                }

                System.Threading.Thread.Sleep(100);

                
                if (Console.KeyAvailable == true)       //Key 입력이 있을 경우
                    break;                              //정지

                OutputVal++;
            }

            
            //
            // IO data update stop
            //
            m_Err = m_cFnIo.FNIO_DevIoUpdateStop(hDevice);
            if (m_Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
                return m_Err;


            return 0;
        }


        static void Main(string[] args)
        {
            //
            //Initialize System
            //
            m_Err = m_cFnIo.FNIO_LibInitSystem(ref m_hSystem);
            if (m_Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
            {
                Console.Write("Failed to Initialize the system.\n ");
                return;
            }
            
            //
            //Create Device Infomation Structure
            //
            CrevisFnIO.DEVICEINFOMODBUSTCP2 DeviceInfo = new CrevisFnIO.DEVICEINFOMODBUSTCP2();
            DeviceInfo.IpAddress = new byte[4];


            Console.Write("IP Address : ");
            string ipAddress = Console.ReadLine();
                        
            int i = 0;

            string[] words = ipAddress.Split('.');
            foreach (string word in words)
            {
                DeviceInfo.IpAddress[i] = (byte)(Int32.Parse(word));
                i++;
            }


            //
            //Open Device
            //
            m_Err = m_cFnIo.FNIO_DevOpenDevice(m_hSystem, ref DeviceInfo, CrevisFnIO.MODBUS_TCP, ref m_hDevivce);
            if (m_Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
            {
                Console.Write("Failed to open the device.\n ");
                m_cFnIo.FNIO_LibFreeSystem(m_hSystem);
                return;
            }




            //
            // IO Data Exchange
            //
            m_Err = IoDataControls(m_hDevivce);
            if (m_Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
            {
                Console.Write("Failed to exchange I/O data.\n ");
                m_cFnIo.FNIO_LibFreeSystem(m_hSystem);
                return;
            }



            //
            //Close Device
            //
            m_Err = m_cFnIo.FNIO_DevCloseDevice(m_hDevivce);
            if (m_Err != CrevisFnIO.FNIO_ERROR_SUCCESS)
            {
                Console.Write("Failed to close the device.\n ");
                m_cFnIo.FNIO_LibFreeSystem(m_hSystem);
                return;
            }

            
            //
            //Free System
            //
            m_Err = m_cFnIo.FNIO_LibFreeSystem(m_hSystem);

        }
    }
}
