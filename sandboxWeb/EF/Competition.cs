//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace sandboxWeb.EF
{
    using System;
    using System.Collections.Generic;
    
    public partial class Competition
    {
        public int CompetitionId { get; set; }
        public string CompetitionName { get; set; }
        public int OMCompetitionId { get; set; }
        public Nullable<bool> DefaultName { get; set; }
    }
}