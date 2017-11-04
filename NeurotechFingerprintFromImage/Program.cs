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
using Neurotec.Biometrics.Standards;

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

                    //finger.FileName = myFileName;
                subject.Fingers.Add(finger);
                subject.Id = "1";

                //conenction to database 
                var connection = new NClusterBiometricConnection
                {
                    Host = "/Local",
                    AdminPort = 27759
                };
                biometricClient.RemoteConnections.Add(connection);
                NBiometricStatus status;
               
                



                //Set finger template size (recommended, for enroll to database, is large)
                //FacesTemplateSize is not set, so the default empalte size value is used
                biometricClient.FingersTemplateSize = NTemplateSize.Large;
                
                 status = NBiometricStatus.InternalError;
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
                    File.WriteAllBytes("E:\\Fingerprint sample\\General Template", subject.GetTemplateBuffer().ToArray());
                    string templateFile = "E:\\Fingerprint sample\\General Template";
                    finger.FileName = templateFile;
                    
                    //add into database
                    NBiometricTask enrollTask =
               biometricClient.CreateTask(NBiometricOperations.Enroll, subject);
                    biometricClient.PerformTask(enrollTask);
                    status = enrollTask.Status;
                    if (status != NBiometricStatus.Ok)
                    {
                        Console.WriteLine("Enrollment was unsuccessful. Status: {0}.", status);
                        if (enrollTask.Error != null) throw enrollTask.Error;
                        return -1;
                    }
                    Console.WriteLine(String.Format("Enrollment was successful."));
                    Console.ReadLine();



                    //BDifStandard.ISO
                    File.WriteAllBytes("E:\\Fingerprint sample\\ISO Template", subject.GetTemplateBuffer(CbeffBiometricOrganizations.IsoIecJtc1SC37Biometrics,
                                     CbeffBdbFormatIdentifiers.IsoIecJtc1SC37BiometricsFingerMinutiaeRecordFormat,
                                     FMRecord.VersionIsoCurrent).ToArray());
                    //BDifStandard.ANSI
                    File.WriteAllBytes("E:\\Fingerprint sample\\ANSI Template", subject.GetTemplateBuffer(CbeffBiometricOrganizations.IncitsTCM1Biometrics,
                                        CbeffBdbFormatIdentifiers.IncitsTCM1BiometricsFingerMinutiaeU,
                                          FMRecord.VersionAnsiCurrent).ToArray());

                   Console.WriteLine("template saved successfully");

                   // biometricClient.SetDatabaseConnectionToOdbc("Dsn=mssql_dsn;UID=sa;PWD=ddm@TT", "subjects");

                    



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
