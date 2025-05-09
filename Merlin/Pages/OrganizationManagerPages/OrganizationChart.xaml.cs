using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using MerlinAdministrator.Models;

namespace MerlinAdministrator.Pages.OrganizationManagerPages
{
    public partial class OrganizationChart : Page
    {
        private readonly DatabaseHelper dbHelper = new();

        public OrganizationChart()
        {
            InitializeComponent();
            LoadOrganizationHierarchy();
        }

        private void LoadOrganizationHierarchy()
        {
            var divisions = new List<DivisionNode>();

            using var conn = new SqlConnection(dbHelper.GetConnectionString());
            conn.Open();

            // Load Divisions
            var cmd = new SqlCommand("SELECT DivisionID, DivisionName, DivisionSupervisorID FROM Divisions", conn);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    divisions.Add(new DivisionNode
                    {
                        Division = new Division
                        {
                            DivisionID = reader["DivisionID"].ToString(),
                            DivisionName = reader["DivisionName"].ToString(),
                            DivisionSupervisorID = reader["DivisionSupervisorID"].ToString()
                        }
                    });
                }
            }

            foreach (var division in divisions)
            {
                // Load Markets
                cmd = new SqlCommand("SELECT MarketID, MarketName, MarketSupervisorID FROM Markets WHERE DivisionID = @DivisionID", conn);
                cmd.Parameters.AddWithValue("@DivisionID", division.Division.DivisionID);
                using var mReader = cmd.ExecuteReader();
                while (mReader.Read())
                {
                    var market = new MarketNode
                    {
                        Market = new Market
                        {
                            MarketID = mReader["MarketID"].ToString(),
                            MarketName = mReader["MarketName"].ToString(),
                            MarketSupervisorID = mReader["MarketSupervisorID"].ToString(),
                            DivisionID = division.Division.DivisionID
                        }
                    };
                    division.Markets.Add(market);
                }
                mReader.Close();

                foreach (var market in division.Markets)
                {
                    // Load Regions
                    cmd = new SqlCommand("SELECT RegionID, RegionName, RegionSupervisorID FROM Regions WHERE MarketID = @MarketID", conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@MarketID", market.Market.MarketID);
                    using var rReader = cmd.ExecuteReader();
                    while (rReader.Read())
                    {
                        var region = new RegionNode
                        {
                            Region = new Region
                            {
                                RegionID = rReader["RegionID"].ToString(),
                                RegionName = rReader["RegionName"].ToString(),
                                RegionSupervisorID = rReader["RegionSupervisorID"].ToString(),
                                MarketID = market.Market.MarketID,
                                DivisionID = division.Division.DivisionID
                            }
                        };
                        market.Regions.Add(region);
                    }
                    rReader.Close();

                    foreach (var region in market.Regions)
                    {
                        // Load Districts
                        cmd = new SqlCommand("SELECT DistrictID, DistrictName, DistrictSupervisorID FROM Districts WHERE RegionID = @RegionID", conn);
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@RegionID", region.Region.RegionID);
                        using var dReader = cmd.ExecuteReader();
                        while (dReader.Read())
                        {
                            var district = new DistrictNode
                            {
                                District = new District
                                {
                                    DistrictID = dReader["DistrictID"].ToString(),
                                    DistrictName = dReader["DistrictName"].ToString(),
                                    DistrictSupervisorID = dReader["DistrictSupervisorID"].ToString(),
                                    RegionID = region.Region.RegionID,
                                    MarketID = market.Market.MarketID,
                                    DivisionID = division.Division.DivisionID
                                }
                            };
                            region.Districts.Add(district);
                        }
                        dReader.Close();

                        foreach (var district in region.Districts)
                        {
                            // Load Locations
                            cmd = new SqlCommand("SELECT * FROM Location WHERE LocationDistrictID = @DistrictID", conn);
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@DistrictID", district.District.DistrictID);
                            using var lReader = cmd.ExecuteReader();
                            while (lReader.Read())
                            {
                                district.Locations.Add(new Location
                                {
                                    LocationID = lReader["LocationID"].ToString().Trim(),
                                    LocationStreetAddress = lReader["LocationStreetAddress"].ToString().Trim(),
                                    LocationCity = lReader["LocationCity"].ToString().Trim(),
                                    LocationState = lReader["LocationState"].ToString().Trim(),
                                    LocationZIP = lReader["LocationZIP"].ToString().Trim(),
                                    LocationPhoneNumber = lReader["LocationPhoneNumber"].ToString().Trim(),
                                    LocationManagerID = lReader["LocationManagerID"].ToString().Trim(),
                                    LocationType = lReader["LocationType"].ToString().Trim(),
                                    LocationIsTradeHold = Convert.ToBoolean(lReader["LocationIsTradeHold"]),
                                    LocationTradeHoldDuration = Convert.ToInt32(lReader["LocationTradeHoldDuration"])
                                });
                            }
                        }
                    }
                }
            }

            OrganizationTree.ItemsSource = divisions;
        }
    }
}
