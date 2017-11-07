using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neurotec.Licensing;
using Neurotec.Biometrics.Client;
using Neurotec.Biometrics;
using System.IO;
using Neurotec.Biometrics.Standards;

namespace NeurotechFingerprintFromImage
{
    class Program
    {
        static int Main(string[] args)
        {
            BdifStandard standard = BdifStandard.Unspecified;


            //Licenses obtain for components

            string components ="Biometrics.FaceExtraction,Biometrics.FingerExtraction,Devices.Cameras";
            try
            {
                if (!NLicense.ObtainComponents("/local", 5000, components))
                {

                    throw new ApplicationException(string.Format("Could not obtain licenses for components: { 0 }", components));
                  
                }

            }catch
            {
                Console.WriteLine(components);
                
            }
         
            using (var biometricClient = new NBiometricClient { UseDeviceManager = true })
            using (var deviceManager = biometricClient.DeviceManager)
            using (var subject = new NSubject())
            

            using (var finger = new NFinger())
            {
                // conenction to database
                // biometricClient.SetDatabaseConnectionToOdbc("Dsn=mssql_dsn;UID=sa;PWD=ddm@TT", "subjects");

                //conenction to database through NServer
                var connection = new NClusterBiometricConnection
                {
                    Host = "127.0.0.1",
                    AdminPort = 24932   //admin listen port
                };
                biometricClient.RemoteConnections.Add(connection);
                ;

                //image location to create template 
                string imageFile = "E:\\Fingerprint sample\\Latest Sample\\Fourth Finger.jpg";
              
                finger.FileName = imageFile;
                subject.Fingers.Add(finger);
                subject.Id = "2512"; //ID number in the database

                //Set finger template size (recommended, for enroll to database, is large)

                biometricClient.FingersTemplateSize = NTemplateSize.Large;
                
                NBiometricStatus status = NBiometricStatus.InternalError;

                //creates template using the image
                status = biometricClient.CreateTemplate(subject);
                if (status == NBiometricStatus.Ok)
                {
                    //ISO or ANSI template stadard can be set before extraction
                    Console.WriteLine("{0} template extracted.", standard == BdifStandard.Iso ?
                    "ISO" : standard == BdifStandard.Ansi ? "ANSI" : "Proprietary");
                    Console.WriteLine("Template extracted");
                    // save image to file
                    using (var image = subject.Fingers[0].Image)
                    {
                       //save image file in this path 
                        image.Save("E:\\Fingerprint sample\\new.jpg");
                        Console.WriteLine("image saved successfully");
                      
                    }

                    if (standard == BdifStandard.Iso)
                    {   //create BDifStandard.ISO template
                        File.WriteAllBytes("E:\\Fingerprint sample\\Latest Sample\\Fourth Template Generated ISO", subject.GetTemplateBuffer(CbeffBiometricOrganizations.IsoIecJtc1SC37Biometrics,
                            CbeffBdbFormatIdentifiers.IsoIecJtc1SC37BiometricsFingerMinutiaeRecordFormat,
                            FMRecord.VersionIsoCurrent).ToArray());
                    }
                    else if (standard == BdifStandard.Ansi)
                    {
                        //create BDifStandard.ANSI template
                        File.WriteAllBytes("E:\\Fingerprint sample\\Latest Sample\\Fourth Template Generated ANSI", subject.GetTemplateBuffer(CbeffBiometricOrganizations.IncitsTCM1Biometrics,
                            CbeffBdbFormatIdentifiers.IncitsTCM1BiometricsFingerMinutiaeU,
                            FMRecord.VersionAnsiCurrent).ToArray());
                    }
                    else
                    {
                        //create general template  
                        File.WriteAllBytes("E:\\Fingerprint sample\\Latest Sample\\Fourth Finger Template Generated", subject.GetTemplateBuffer().ToArray());
                    }
  
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
