using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using System.Text;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Grow_Runner
{
	public sealed partial class MainPage : Page
	{
		public static String logPath = "C:\\GrowRunnerLog.txt";
		//public static Stream stream = File.Open(logPath, FileMode.OpenOrCreate);
		//public static StringBuilder sb = new StringBuilder();
		public static UsbDevice usbConnection;
		public static UsbEndpointReader usbRead;
		public static UsbEndpointWriter usbWrite;
		#region SET YOUR USB Vendor and Product ID!
		public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(1234, 1);
		#endregion

		public MainPage ()
		{
			//ReadWriteAsync.usbConnection();
			this.InitializeComponent();
		}
		public static void usbReadWrite ()
		{

		}

		internal class ReadWriteAsync
		{
			public static UsbDevice usbBridge;

			#region SET YOUR USB Vendor and Product ID!

			public static UsbDeviceFinder usbFinder = new UsbDeviceFinder(1234, 1);

			#endregion

			public static void usbConnection ()
			{
				ErrorCode ec = ErrorCode.None;
				string[] args = new string[] { "Test of USB bridge" };

				try
				{
					// Find and open the usb device.
					usbBridge = UsbDevice.OpenUsbDevice(usbFinder);

					// If the device is open and ready
					if (usbBridge == null) throw new Exception("Device Not Found.");

					// If this is a "whole" usb device (libusb-win32, linux libusb)
					// it will have an IUsbDevice interface. If not (WinUSB) the 
					// variable will be null indicating this is an interface of a 
					// device.
					IUsbDevice wholeUsbDevice = usbBridge as IUsbDevice;
					if (!ReferenceEquals(wholeUsbDevice, null))
					{
						// This is a "whole" USB device. Before it can be used, 
						// the desired configuration and interface must be selected.

						// Select config #1
						wholeUsbDevice.SetConfiguration(1);

						// Claim interface #0.
						wholeUsbDevice.ClaimInterface(0);
					}

					// open read endpoint 1.
					UsbEndpointReader readUSB = usbBridge.OpenEndpointReader(ReadEndpointID.Ep01);

					// open write endpoint 1.
					UsbEndpointWriter writeUSB = usbBridge.OpenEndpointWriter(WriteEndpointID.Ep01);

					// the write test data.
					string testWriteString = "ABCDEFGH";

					ErrorCode ecWrite;
					ErrorCode ecRead;
					int transferredOut;
					int transferredIn;
					UsbTransfer usbWriteTransfer;
					UsbTransfer usbReadTransfer;
					byte[] bytesToSend = Encoding.ASCII.GetBytes(testWriteString);
					byte[] readBuffer = new byte[1024];
					int testCount = 0;
					do
					{
						// Create and submit transfer
						ecRead = readUSB.SubmitAsyncTransfer(readBuffer, 0, readBuffer.Length, 100, out usbReadTransfer);
						if (ecRead != ErrorCode.None) throw new Exception("Submit Async Read Failed.");

						ecWrite = writeUSB.SubmitAsyncTransfer(bytesToSend, 0, bytesToSend.Length, 100, out usbWriteTransfer);
						if (ecWrite != ErrorCode.None)
						{
							usbReadTransfer.Dispose();
							throw new Exception("Submit Async Write Failed.");
						}

						System.Threading.WaitHandle.WaitAll(new System.Threading.WaitHandle[] { usbWriteTransfer.AsyncWaitHandle, usbReadTransfer.AsyncWaitHandle });
						if (!usbWriteTransfer.IsCompleted) usbWriteTransfer.Cancel();
						if (!usbReadTransfer.IsCompleted) usbReadTransfer.Cancel();

						ecWrite = usbWriteTransfer.Wait(out transferredOut);
						ecRead = usbReadTransfer.Wait(out transferredIn);

						usbWriteTransfer.Dispose();
						usbReadTransfer.Dispose();

						//sb.AppendLine("Read  :{0} ErrorCode:{1} - " + transferredIn + " - " + ecRead);
						//sb.AppendLine("Write :{0} ErrorCode:{1} - " + transferredOut + " - " + ecWrite);
						//sb.AppendLine("Data  :" + Encoding.ASCII.GetString(readBuffer, 0, transferredIn));
						testCount++;
					} while (testCount < 5);
					//sb.AppendLine("\r\nDone!\r\n");
				}
				catch (Exception ex)
				{
					//sb.AppendLine();
					//sb.AppendLine((ec != ErrorCode.None ? ec + ":" : String.Empty) + ex.Message);
				}
				finally
				{
					if (usbBridge != null)
					{
						if (usbBridge.IsOpen)
						{
							// If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
							// it exposes an IUsbDevice interface. If not (WinUSB) the 
							// 'wholeUsbDevice' variable will be null indicating this is 
							// an interface of a device; it does not require or support 
							// configuration and interface selection.
							IUsbDevice wholeUsbDevice = usbBridge as IUsbDevice;
							if (!ReferenceEquals(wholeUsbDevice, null))
							{
								// Release interface #0.
								wholeUsbDevice.ReleaseInterface(0);
							}

							usbBridge.Close();
						}
						usbBridge = null;
						//stream.Write(Encoding.ASCII.GetBytes(sb.ToString()), 0, sb.Length);
						// Free usb resources
						UsbDevice.Exit();
					}
				}
			}
		}
	}
}
