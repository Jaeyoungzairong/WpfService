using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace WpfService.Models
{
    public class Assays
    {
        public static Assays Instance { get; } = new Assays();
        private Assays() { }

        [JsonProperty("dna")]
        public  List<Assay> Dna { get; set; }

        [JsonProperty("proteins")]
        public  List<Assay> Proteins { get; set; }

        [JsonProperty("cuvette")]
        public  List<Assay> Cuvette { get; set; }

        public void Load()
        {
            string path = Path.Combine(Application.StartupPath, "assays.json");
            string json = File.ReadAllText(path);
            Assays assays = JsonConvert.DeserializeObject<Assays>(json);

            this.Dna = assays.Dna;
            this.Proteins = assays.Proteins;
            this.Cuvette = assays.Cuvette;
        }

        public static Assays LoadFromFile(string path)
        {
            string json = File.ReadAllText(path);

            Assays assays = JsonConvert.DeserializeObject<Assays>(json);
            if (assays == null)
                throw new InvalidDataException("Assays JSON deserialization returned null.");

            return assays;
        }

        public Assay GetDnaAssay(string subType) =>
            Dna.First(r => r.AssaySubType == subType);

        public Assay GetProteinsAssay(string subType) =>
            Proteins.First(r => r.AssaySubType == subType);

        public Assay GetCuvetteAssay(string subType) => Cuvette.First(r => r.AssaySubType == subType);

        public Assay GetAssay(string subType)
        {
            return Dna.FirstOrDefault(r => r.AssaySubType == subType)
                ?? Proteins.FirstOrDefault(r => r.AssaySubType == subType)
                ?? Cuvette.FirstOrDefault(r => r.AssaySubType == subType);
        }
    }

    public class Assay
    {
        [JsonProperty("assayType")]
        public string AssayType { get; set; } = "";

        [JsonProperty("assaySubType")]
        public string AssaySubType { get; set; } = "";

        [JsonProperty("label")]
        public string Label { get; set; } = "";

        [JsonProperty("image")]
        public string Image { get; set; } = "";

        [JsonProperty("rangeFrom")]
        public int RangeFrom { get; set; }

        [JsonProperty("rangeTo")]
        public int RangeTo { get; set; }

        [JsonProperty("peakWL")]
        public int PeakWL { get; set; }

        [JsonProperty("mainWL")]
        public int MainWL { get; set; }

        [JsonProperty("secondaryWL")]
        public int SecondaryWL { get; set; }

        [JsonProperty("extinctionCoefficient")]
        public double ExtinctionCoefficient { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; } = "";
    }
}
