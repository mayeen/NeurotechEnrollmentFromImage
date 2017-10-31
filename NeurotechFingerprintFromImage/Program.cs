using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neurotec.Licensing;
using Neurotec.Biometrics.Client;
using Neurotec.Biometrics;
using System.IO;
using Neurotec.Gui;
using Neurotec.Biometrics.Gui;


namespace NeurotechFingerprintFromImage
{
    class Program
    {
        private static bool fileAccessible(string filePath)
        {
            return File.Exists(filePath);

        }
        static int Main(string[] args)
        {
            //Licenses obtain for components

            string components =       "Biometrics.FaceExtraction,Biometrics.FingerExtraction,Devices.Cameras";
            try
            {
                if (!NLicense.ObtainComponents("/local", 5000, components))
                {

                    throw new ApplicationException(string.Format("Could not obtain licenses for components: { 0 }", components));
                   // Console.WriteLine("obtained");
                }

            }catch
            {
                Console.WriteLine(components);
                //Console.ReadLine();
            }
            
            //NBiometricClient biometricClient = new NBiometricClient();
            using (var biometricClient = new NBiometricClient { UseDeviceManager = true })
            using (var deviceManager = biometricClient.DeviceManager)
            using (var subject = new NSubject())
            using (var face = new NFace())

            using (var finger = new NFinger())
            {
             //   Console.WriteLine("{0}",args[0]);
               // Console.ReadLine();

                string myFileName = "E:\\Fingerprint sample\\012_3_3.jpg";
               // string sampleFile = "012_3_2.jpg";
               bool a=  Directory.Exists(myFileName);

              bool b=  fileAccessible(myFileName);



                    //args[0] is file name or full path to a file where fingerprint image is saved

                    finger.FileName = myFileName;
                subject.Fingers.Add(finger);
                //Set finger template size (recommended, for enroll to database, is large)

                //FacesTemplateSize is not set, so the default empalte size value is used
                biometricClient.FingersTemplateSize = NTemplateSize.Large;
                
                NBiometricStatus status = NBiometricStatus.InternalError;
                status = biometricClient.CreateTemplate(subject);
                if (status == NBiometricStatus.Ok)
                {
                    Console.WriteLine("Template extracted");
                    // save image to file
                    using (var image = subject.Fingers[0].Image)
                    {
                       
                        image.Save("E:\\Fingerprint sample\\new.jpg");
                        Console.WriteLine("image saved successfully");
                      
                    }
                    //args[1] contains file name to save template
                    File.WriteAllBytes("E:\\Fingerprint sample\\new Template", subject.GetTemplateBuffer().ToArray());
                    Console.WriteLine("template saved successfully");
                }
                else
                {
                    Console.WriteLine("Extraction failed! Status: {0}", status);
                    return -1;
                }
            }


                return 0;
        }
    }
}
