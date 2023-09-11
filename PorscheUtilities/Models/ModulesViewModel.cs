﻿namespace PorscheUtilities.Models
{
    public class ModulesViewModel
    {
        public int Id { get; set; }
        public string? ModuleName { get; set; }
        public string? ModuleDescription { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
