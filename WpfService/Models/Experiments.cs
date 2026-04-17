using System;
using System.Collections.Generic;

namespace WpfService.Models
{
    public class ExperimentDateGroup
    {
        public DateTime Date { get; set; }
        public List<Experiment> Experiments { get; set; }
    }

    public class Experiment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AssayType { get; set; }
        public string ExperimentType { get; set; }
        public Dictionary<string, object> InputData { get; set; }
        public DateTime CreatedAt { get; set; }
        public int MeasurementCount { get; set; }
        //public List<Measurement> Measurements { get; set; }
    }

    public class Measurement
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string MeasurementName { get; set; }
        public Dictionary<string, object> MeasurementData { get; set; }
        public List<double> Data1 { get; set; }
        public List<double> Data2 { get; set; }
        public List<double> Data3 { get; set; }
    }
}
