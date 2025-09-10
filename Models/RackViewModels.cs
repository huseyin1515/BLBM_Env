namespace BLBM_ENV.Models
{
    // Bir kabin içindeki tek bir "U" birimini temsil eder.
    public class RackUnitViewModel
    {
        public int U_Number { get; set; }
        public List<Envanter> OccupyingServers { get; set; } = new List<Envanter>();
        public bool IsOccupied => OccupyingServers.Any();
    }

    // RackView sayfasının tamamı için ana model.
    public class RackVisualizationViewModel
    {
        public List<string> AllLocations { get; set; } = new List<string>();
        public string SelectedLocation { get; set; } = string.Empty;

        // Key: Kabin Adı (Örn: "101 (Ön)"), Value: O kabine ait 42U'luk liste
        public Dictionary<string, List<RackUnitViewModel>> Racks { get; set; } = new Dictionary<string, List<RackUnitViewModel>>();
    }
}