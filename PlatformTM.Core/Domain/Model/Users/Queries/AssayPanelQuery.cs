﻿using System.Collections.Generic;

namespace PlatformTM.Core.Domain.Model.Users.Queries
{
    public class AssayPanelQuery
    {
        public int AssayId { get; set; }
        public string AssayName { get; set; }
        //public List<ObservationQuery> FeatureQuery { get; set; }
        public List<Query> SampleQueries { get; set; }
        //public List<ObservationQuery> ObservationMeasureQuery { get; set; }

        public AssayPanelQuery()
        {
            SampleQueries = new List<Query>();
        }

    }
}
