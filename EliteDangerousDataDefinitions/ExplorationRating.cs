﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EliteDangerousDataDefinitions
{
    /// <summary>
    /// Exploration ratings
    /// </summary>
    public class ExplorationRating
    {
        private static readonly List<ExplorationRating> RATINGS = new List<ExplorationRating>();

        public string edname { get; private set; }

        public string name { get; private set; }

        public int rank { get; private set; }

        private ExplorationRating(string edname, int rank, string name)
        {
            this.edname = edname;
            this.rank = rank;
            this.name = name;

            RATINGS.Add(this);
        }

        public static readonly ExplorationRating Aimless = new ExplorationRating("Aimless", 0, "Aimless");
        public static readonly ExplorationRating MostlyAimless = new ExplorationRating("MostlyAimless", 1, "Mostly Aimless");
        public static readonly ExplorationRating Scout = new ExplorationRating("Scout", 2, "Scout");
        public static readonly ExplorationRating Surveyor = new ExplorationRating("Surveyor", 3, "Surveyor");
        public static readonly ExplorationRating Explorer = new ExplorationRating("Explorer", 4, "Explorer");
        public static readonly ExplorationRating Pathfinder = new ExplorationRating("Pathfinder", 5, "Pathfinder");
        public static readonly ExplorationRating Ranger = new ExplorationRating("Ranger", 6, "Ranger");
        public static readonly ExplorationRating Pioneer = new ExplorationRating("Pioneer", 7, "Pioneer");
        public static readonly ExplorationRating Elite = new ExplorationRating("Elite", 8, "Elite");

        public static ExplorationRating FromName(string from)
        {
            ExplorationRating result = RATINGS.First(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown Exploration Rating name " + from);
            }
            return result;
        }

        public static ExplorationRating FromEDName(string from)
        {
            ExplorationRating result = RATINGS.First(v => v.edname == from);
            if (result == null)
            {
                Logging.Report("Unknown Exploration Rating ED name " + from);
            }
            return result;
        }

        public static ExplorationRating FromRank(int from)
        {
            ExplorationRating result = RATINGS.First(v => v.rank == from);
            if (result == null)
            {
                Logging.Report("Unknown Exploration Rating rank " + from);
            }
            return result;
        }
    }
}
