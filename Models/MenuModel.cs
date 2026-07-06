namespace VGN_CRM_CORE.Models
{
    /// <summary>
    /// Represents a single menu item from Web_ModuleAllocation / Web_UserRights.
    /// Hierarchy: Department → ModuleType (Category) → ModuleCaptionName (Sub-item)
    /// </summary>
    public class MenuModel
    {
        public string ModuleType       { get; set; }   // MASTER, ACTIVITIES, DASHBOARD, REPORTS
        public string Department       { get; set; }   // ADMINISTRATION, HUMAN RESOURCES, etc.
        public string ModuleCaptionName { get; set; }   // e.g. "Visitor Entry List"
        public string ControllerName   { get; set; }
        public string ActionName       { get; set; }
    }
}
