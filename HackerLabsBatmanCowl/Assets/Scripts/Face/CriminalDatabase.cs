using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.IO;
using UnityEngine;
using System.Net.Http;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading.Tasks;

public class CriminalDatabase : MonoBehaviour
{
    List<Criminal_t> criminals;

    const string faceAPIKey = "f6633cc8206a495199dd7bcb6053afe8";

    const string personGroupID = "BatmanCriminalDatabase";

    void OnEnable()
    {
        ServicePointManager.ServerCertificateValidationCallback = MonoSecurityBypass;

        criminals = new List<Criminal_t>();

        InitializeDatabase();
    }

    public bool MonoSecurityBypass(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        bool isOk = true;
        // If there are errors in the certificate chain,
        // look at each error to determine the cause.
        if (sslPolicyErrors != SslPolicyErrors.None)
        {
            for (int i = 0; i < chain.ChainStatus.Length; i++)
            {
                if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    continue;
                }
                chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                bool chainIsValid = chain.Build((X509Certificate2)certificate);
                if (!chainIsValid)
                {
                    isOk = false;
                    break;
                }
            }
        }
        return isOk;
    }

    async Task<bool> CheckExistingPersonGroup()
    {
        HttpClient httpClient = new HttpClient();

        string url = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/persongroups/batcowldb";

        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", faceAPIKey);

        HttpResponseMessage responseMessage;

        responseMessage = await httpClient.GetAsync(url);

        if (responseMessage.StatusCode == HttpStatusCode.OK)
        {
            Debug.LogFormat("Request returned {0}, Person Group Exists.", responseMessage.StatusCode);
            return true;
        }
        else
        {
            Debug.LogFormat("Request returned {0}, Person Group Will Be Created.", responseMessage.StatusCode);
            return false;
        }
    }

    async Task<bool> InitializePersonGroup()
    {
        if (await CheckExistingPersonGroup())
            return true;

        HttpClient httpClient = new HttpClient();

        string url = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/persongroups/batcowldb";

        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", faceAPIKey);

        HttpResponseMessage responseMessage;

        byte[] data = System.Text.Encoding.UTF8.GetBytes("{ \"name\" : \"" + personGroupID + "\" }");

        using (ByteArrayContent c = new ByteArrayContent(data))
        {
            c.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            responseMessage = await httpClient.PutAsync(url, c);
        }

        Debug.Log(responseMessage.ToString());

        if (responseMessage.IsSuccessStatusCode)
            return true;

        return false;
    }

    void Update()
    {

    }

    async void InitializeDatabase()
    {
        if (await InitializePersonGroup())
        {
            if (File.Exists(Path.Combine(Application.dataPath, "criminalDatabase.batdb")))
            {
                FileStream stream = new FileStream(Path.Combine(Application.dataPath, "criminalDatabase.batdb"), FileMode.Open, FileAccess.Read);
                BinaryFormatter formatter = new BinaryFormatter();

                criminals = (List<Criminal_t>)formatter.Deserialize(stream);

                stream.Close();
            }
            else
            {
                CriminalInformation_t crim = new CriminalInformation_t();

                crim.Crimes.Add(new Crime_t(Crime_t.CrimeType.Murder, 674));
                crim.Crimes.Add(new Crime_t(Crime_t.CrimeType.Larceny, 238));
                crim.Warrants.Add(new Warrant_t(Warrant_t.WarrantType.Arrest, new Crime_t(Crime_t.CrimeType.Murder, 21)));
                crim.Warrants.Add(new Warrant_t(Warrant_t.WarrantType.Arrest, new Crime_t(Crime_t.CrimeType.Larceny, 10)));

                criminals.Add(new Criminal_t("Jonathan Ribarro", "Deathbringer", 20, Path.Combine(Application.streamingAssetsPath, "jribarro.jpg"), crim));

                crim = new CriminalInformation_t();

                crim.Crimes.Add(new Crime_t(Crime_t.CrimeType.Arson, 3));
                crim.Crimes.Add(new Crime_t(Crime_t.CrimeType.Assault, 7));
                crim.Crimes.Add(new Crime_t(Crime_t.CrimeType.Pickpocketing, 956));
                crim.Warrants.Add(new Warrant_t(Warrant_t.WarrantType.WantedQuestioning, new Crime_t(Crime_t.CrimeType.GrandLarceny, 2)));

                criminals.Add(new Criminal_t("Dusty Langeberg", "Firehazard", 18, Path.Combine(Application.streamingAssetsPath, "dlangeberg.jpg"), crim));

                crim = new CriminalInformation_t();

                crim.Crimes.Add(new Crime_t(Crime_t.CrimeType.Larceny, 27));
                crim.Crimes.Add(new Crime_t(Crime_t.CrimeType.ImpersonationOfALawEnforcementOfficer, 12));
                crim.Crimes.Add(new Crime_t(Crime_t.CrimeType.CriminalImpersonation, 27));
                crim.Crimes.Add(new Crime_t(Crime_t.CrimeType.HotProwlBurglary, 69));
                crim.Crimes.Add(new Crime_t(Crime_t.CrimeType.TooFabulous, 348));
                crim.Warrants.Add(new Warrant_t(Warrant_t.WarrantType.Arrest, new Crime_t(Crime_t.CrimeType.Abduction, 8)));

                criminals.Add(new Criminal_t("Shae Ryan", "The Chameleon", 26, Path.Combine(Application.streamingAssetsPath, "sryan.jpg"), crim));

                //crim = new CriminalInformation_t();

                //criminals.Add(new Criminal_t("", "Cameraman", 0, Path.Combine(Application.streamingAssetsPath, ".jpg"), crim));

                //crim = new CriminalInformation_t();

                //criminals.Add(new Criminal_t("Abhijeet Malamkar", "", 0, Path.Combine(Application.streamingAssetsPath, "amalamkar.jpg"), crim));

                //crim = new CriminalInformation_t();

                //criminals.Add(new Criminal_t("Mitchell Hartwell", "", 0, Path.Combine(Application.streamingAssetsPath, "mhartwell.jpg"), crim));

                for (int i = 0; i < criminals.Count; i++)
                {
                    criminals[i] = await AddPersonToGroup(criminals[i]);
                    criminals[i] = await AddFaceToPerson(criminals[i]);
                }

                FileStream stream = new FileStream(Path.Combine(Application.dataPath, "criminalDatabase.batdb"), FileMode.OpenOrCreate, FileAccess.Write);
                BinaryFormatter formatter = new BinaryFormatter();

                formatter.Serialize(stream, criminals);

                stream.Close();
            }
            if (await TrainGroup())
                Debug.Log("Training Initialized");
            else
                Debug.Log("Training Run or Failed.");

            int x = criminals.Count;
        }
    }

    async void DeletePersonGroup()
    {
        string url = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/persongroups/batcowldb";

        HttpClient httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", faceAPIKey);

        HttpResponseMessage response = await httpClient.DeleteAsync(url);

        string c = await response.Content.ReadAsStringAsync();

        Debug.Log(c);
    }

    async Task<bool> CheckForPerson(string personID)
    {
        HttpClient httpClient = new HttpClient();

        string url = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/persongroups/batcowldb/persons";

        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", faceAPIKey);

        HttpResponseMessage responseMessage;

        responseMessage = await httpClient.GetAsync(url);

        string content = await responseMessage.Content.ReadAsStringAsync();

        Debug.LogFormat("Content: {0}", content);

        if (content.Contains(personID))
            return true;
        else
            return false;
    }

    async Task<Criminal_t> AddPersonToGroup(Criminal_t criminal)
    {
        //if (await CheckForPerson(criminal.CriminalName))
        //    return criminal;

        HttpClient httpClient = new HttpClient();

        string url = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/persongroups/batcowldb/persons";

        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", faceAPIKey);

        HttpResponseMessage responseMessage;

        byte[] data = System.Text.Encoding.UTF8.GetBytes("{ \"name\" : \"" + criminal.CriminalName + "\" }");

        Debug.Log("{ \"name\" : \"" + criminal.CriminalName + "\" }");

        using (ByteArrayContent c = new ByteArrayContent(data))
        {
            c.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            responseMessage = await httpClient.PostAsync(url, c);
        }

        string jstring = await responseMessage.Content.ReadAsStringAsync();

        Debug.Log(jstring);

        JSONObject jObj = new JSONObject(jstring);

        criminal.CriminalID = jObj.GetField("personId").ToString().Replace("\"", "");

        return criminal;
    }

    async Task<bool> CheckForExistingFace(string personID)
    {
        HttpClient httpClient = new HttpClient();

        string url = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/persongroups/batcowldb/persons/" + personID;

        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", faceAPIKey);

        HttpResponseMessage responseMessage = await httpClient.GetAsync(url);

        string content = await responseMessage.Content.ReadAsStringAsync();

        JSONObject jObj = new JSONObject(content);

        if (jObj.list.Count < 1)
            return true;

        return false;
    }

    async Task<Criminal_t> AddFaceToPerson(Criminal_t criminal)
    {
        if (await CheckForExistingFace(criminal.CriminalID))
            return criminal;

        string url = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/persongroups/batcowldb/persons/" + criminal.CriminalID + "/persistedFaces";

        HttpClient httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", faceAPIKey);

        string body = "{" + criminal.CriminalImage.ToString() + "}";

        byte[] byteData = System.Text.Encoding.UTF8.GetBytes(body);

        HttpResponseMessage responseMessage;

        using (var content = new ByteArrayContent(byteData))
        {
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            responseMessage = await httpClient.PostAsync(url, content);
        }

        string jstring = await responseMessage.Content.ReadAsStringAsync();

        Debug.Log(jstring);

        JSONObject jObj = new JSONObject(jstring);

        if (jObj.list.Count > 0)
            criminal.CriminalFaceID = new string(jObj[0].GetField("persistedFaceIds").ToString().ToCharArray());
        else
            criminal.CriminalFaceID = new string(jObj.GetField("persistedFaceIds").ToString().ToCharArray());

        return criminal;
    }

    async Task<bool> TrainGroup()
    {
        string url = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/persongroups/batcowldb/train";

        HttpClient httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", faceAPIKey);

        HttpResponseMessage responseMessage;

        byte[] byteData = System.Text.Encoding.UTF8.GetBytes("{}");

        using (var content = new ByteArrayContent(byteData))
        {
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            responseMessage = await httpClient.PostAsync(url, content);
        }

        if (responseMessage.IsSuccessStatusCode)
            return true;

        return false;
    }

    async Task<string[]> DetectFace(byte[] image)
    {
        string url = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/detect";

        HttpClient httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", faceAPIKey);

        HttpResponseMessage responseMessage;

        Debug.Log(image.Length);

        byte[] byteData = System.Text.Encoding.UTF8.GetBytes("{" + image.ToString() + "}");

        using (var content = new ByteArrayContent(byteData))
        {
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            //content.Headers.ContentLength = image.LongLength;
            responseMessage = await httpClient.PostAsync(url, content);
        }

        string response = await responseMessage.Content.ReadAsStringAsync();

        JSONObject obj = new JSONObject(response);

        string[] faceIds = new string[obj.list.Count];

        Debug.Log(response);

        for (int i = 0; i < obj.list.Count; i++)
        {
            faceIds[i] = obj[i].GetField("faceId").ToString().Replace("\"", "");
        }

        return faceIds;
    }

    async Task<Criminal_t> IdentifyCriminal(string faceID)
    {
        string url = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/identify";

        HttpClient httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", faceAPIKey);

        HttpResponseMessage responseMessage;

        string body = "{ \"personGroupId\":\"batcowldb\", \"faceIds\":[ \"" + faceID + "\" ], \"maxNumOfCandidatesReturned\":1, \"confidenceThreshold\": 0.5 }";

        byte[] byteData = System.Text.Encoding.UTF8.GetBytes(body);

        using (var content = new ByteArrayContent(byteData))
        {
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            responseMessage = await httpClient.PostAsync(url, content);
        }

        string message = await responseMessage.Content.ReadAsStringAsync();

        JSONObject obj = new JSONObject(message);

        if (obj.list.Count > 0)
        {
            if (obj[0].GetField("candidates")[0].GetField("confidence").f >= 0.7f)
            {
                Criminal_t crim = criminals.Find(a => a.CriminalID.Equals(obj[0].GetField("candidates")[0].GetField("personId").ToString().Replace("\"", "")));

                if (crim != null)
                    return crim;
            }
        }

        return null;
    }

    async Task<Criminal_t> Identify(string[] faces)
    {
        for (int i = 0; i < faces.Length; i++)
        {
            Criminal_t criminal = await IdentifyCriminal(faces[i]);

            if (criminal != null)
                return criminal;
        }

        return null;
    }

    public async Task<Criminal_t> GetCriminalFromImage(byte[] image)
    {
        return await Identify(await DetectFace(image));
    }

    #region classes
    [Serializable]
    public class Criminal_t
    {
        string criminalName;
        string criminalNickname;
        int criminalAge;
        byte[] criminalImage;
        string criminalID;
        string criminalFaceID;

        CriminalInformation_t criminalInformation;

        public Criminal_t(string name, string nickname, int age, string imagePath, CriminalInformation_t crimInfo)
        {
            criminalID = string.Empty;
            criminalFaceID = string.Empty;
            criminalName = name;
            criminalAge = age;
            criminalNickname = nickname;
            criminalImage = File.ReadAllBytes(imagePath);
            criminalInformation = crimInfo;
        }


        public string CriminalName
        {
            get
            {
                return criminalName;
            }

            set
            {
                criminalName = value;
            }
        }

        public int CriminalAge
        {
            get
            {
                return criminalAge;
            }

            set
            {
                criminalAge = value;
            }
        }

        public byte[] CriminalImage
        {
            get
            {
                return criminalImage;
            }

            set
            {
                criminalImage = value;
            }
        }

        public string CriminalID
        {
            get
            {
                return criminalID;
            }

            set
            {
                criminalID = value;
            }
        }

        public string CriminalFaceID
        {
            get
            {
                return criminalFaceID;
            }

            set
            {
                criminalFaceID = value;
            }
        }

        public string CriminalNickname
        {
            get
            {
                return criminalNickname;
            }

            set
            {
                criminalNickname = value;
            }
        }

        private CriminalInformation_t CriminalInformation
        {
            get
            {
                return criminalInformation;
            }

            set
            {
                criminalInformation = value;
            }
        }
    }

    [Serializable]
    public class CriminalInformation_t
    {
        /// <summary>
        /// Crimes this Criminal has been officially charged with
        /// </summary>
        public List<Crime_t> Crimes
        {
            get;
            set;
        }

        /// <summary>
        /// Crimes this Criminal is wanted for
        /// </summary>
        public List<Warrant_t> Warrants
        {
            get;
            set;
        }

        public CriminalInformation_t()
        {
            Crimes = new List<Crime_t>();
            Warrants = new List<Warrant_t>();
        }
    }

    [Serializable]
    public class Warrant_t
    {

        public enum WarrantType
        {
            Arrest,
            WantedQuestioning,
        };

        public WarrantType Warrant
        {
            get;
            set;
        }

        public Crime_t Crime
        {
            get;
            set;
        }

        public Warrant_t(WarrantType t, Crime_t c)
        {
            Warrant = t;
            Crime = c;
        }
    }

    [Serializable]
    public class Crime_t
    {
        public enum CrimeType
        {
            Abduction,
            AnimalCruelty,
            Arson,
            Assault,
            AttemptedMurder,
            Battery,
            Blackmail,
            BlasphemousLibel,
            BombThreat,
            Burglary,
            CapitalMurder,
            ChildAbduction,
            ChildPorn,
            Classicide,
            ConcealingBirth,
            Conspiracy,
            ConspiracyToCommitMurder,
            CriminalImpersonation,
            CriminalPossessionWeapon,
            DangerousDriving,
            DeadlyWeapon,
            DeathThreat,
            Defamation,
            Democide,
            Desertion,
            DisorderlyConduct,
            DomesticViolence,
            DUI,
            DrugPossession,
            Embezzlement,
            EmploymentFraud,
            Endangerment,
            Extortion,
            FailureToAppear,
            FailureToObeyPolice,
            FalseAccounting,
            FalseImprisonment,
            FetalAbduction,
            ForcibleEntry,
            Forgery,
            Fraud,
            Genocide,
            Ghosting,
            GrandLarceny,
            Hacking,
            Harassment,
            HatCrime,
            HotProwlBurglary,
            IdentityCleansing,
            IllegalEntry,
            IllegalImmigration,
            ImpersonationOfALawEnforcementOfficer,
            IndecentExposure,
            InsuranceFraud,
            Intimidation,
            Jaywalking,
            JuryTampering,
            Larceny,
            Loitering,
            MotorVehicleTheft,
            MovingViolation,
            Murder,
            Mutiny,
            ObscenePhoneCall,
            ObstructionOfJustice,
            ObtainingMoneyDeception,
            ObtainingPropertyDeception,
            ObtainingServiceDeception,
            Perjury,
            PettyTheft,
            PhoneCloning,
            Pickpocketing,
            PossessionOfStolenGoods,
            PublicIntoxication,
            PublicNuisance,
            Racket,
            RecklessBurning,
            RecklessEndangerment,
            RefustalToServePublicOffice,
            Robbery,
            Sabotage,
            Sedition,
            Shoplifting,
            Solicitation,
            SolicitationToMurder,
            StagedCrash,
            Stalking,
            Stoyaway,
            Tampering,
            Terrorism,
            TooFabulous,
            Treachery,
            Treason,
            Trespassing,
            UnlawfulAssembly,
            UnlicensedBroadcast,
            Vandalism,
            WarProfiteering,
            WebcamBlackmail,
            WitnessTampering,
        };


        public Crime_t(CrimeType t, int count)
        {
            Crime = t;
            Count = count;
        }

        /// <summary>
        /// The crime committed.
        /// </summary>
        public CrimeType Crime
        {
            get;
            set;
        }

        /// <summary>
        /// Number of times the crime has been committed.
        /// </summary>
        public int Count
        {
            get;
            set;
        }
    }

    #endregion

}
