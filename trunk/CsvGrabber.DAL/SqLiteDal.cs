using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data.SQLite;
using System.Collections;
using System.IO;
using System.Globalization;
using System.Xml.Linq;
using CsvGrabber.Core;

namespace CsvGrabber.DAL {
    public class SqLiteDal : ADONETBase
    {
        public SqLiteDal()
        {
            this.FileName = DalConfigurationManager.DatabasePath;
        }

        public SqLiteDal(string fileName)
        {
            this.FileName = fileName;
        }

        #region Grab Jobs
        public void AddScheduledGrabs(IEnumerable<ScheduledGrab> grabs)
        {
            Execute(conn =>
                        {
                            using (SQLiteTransaction trans = conn.BeginTransaction())
                            {
                                using (SQLiteCommand cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = "INSERT INTO ScheduledGrabs (Interval, Name, IsActive, Schedule, Mode, Parameters) VALUES (@Interval, @Name, @IsActive, @Schedule, @Mode, @Parameters)";
                                    cmd.Parameters.AddWithValue("@Interval", 0);
                                    cmd.Parameters.AddWithValue("@Name", string.Empty);
                                    cmd.Parameters.AddWithValue("@IsActive", false);
                                    cmd.Parameters.AddWithValue("@Schedule", 0);
                                    cmd.Parameters.AddWithValue("@Mode", 0);
                                    cmd.Parameters.AddWithValue("@Parameters", string.Empty);

                                    foreach (ScheduledGrab grab in grabs)
                                    {
                                        cmd.Parameters["@Interval"].Value = grab.Interval;
                                        cmd.Parameters["@Name"].Value = grab.Name;
                                        cmd.Parameters["@IsActive"].Value = grab.IsActive;
                                        cmd.Parameters["@Schedule"].Value = grab.GrabSchedule;
                                        cmd.Parameters["@Mode"].Value = grab.GrabMode;
                                        cmd.Parameters["@Parameters"].Value = grab.GrabParams.Serialize();
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                trans.Commit();
                            }
                        });
        }

        public void EditScheduledGrab(ScheduledGrab grab)
        {
            Execute(conn =>
            {
                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE ScheduledGrabs SET Interval  = @Interval, Name = @Name, IsActive = @IsActive, Schedule = @Schedule, Mode = @Mode, Parameters = @Parameters WHERE GrabID = @GrabID";
                    cmd.Parameters.AddWithValue("@GrabID", grab.GrabID);
                    cmd.Parameters.AddWithValue("@Interval", grab.Interval);
                    cmd.Parameters.AddWithValue("@Name", grab.Name);
                    cmd.Parameters.AddWithValue("@IsActive", grab.IsActive);
                    cmd.Parameters.AddWithValue("@Schedule", grab.GrabSchedule);
                    cmd.Parameters.AddWithValue("@Mode", grab.GrabMode);
                    cmd.Parameters.AddWithValue("@Parameters",grab.GrabParams.Serialize());
                    cmd.ExecuteNonQuery();
                }
            });
        }

        public void DeleteScheduledGrab(ScheduledGrab grab)
        {
            Execute(conn =>
            {
                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.Parameters.AddWithValue("@GrabID", grab.GrabID);

                    cmd.CommandText = "DELETE FROM GrabHistory WHERE GrabID = @GrabID";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "DELETE FROM ScheduledGrabs WHERE GrabID = @GrabID";
                    cmd.ExecuteNonQuery();
                }
            });
        }

        /// <summary>
        /// Duplicates grabs for a range of URLs.
        /// </summary>
        /// <param name="grabToDuplicate">The grab to duplicate.</param>
        /// <param name="urls">The urls.</param>
        public void DuplicateGrabs(ScheduledGrab grabToDuplicate, IEnumerable<string> urls)
        {
            List<string> jobNames = new List<string>();
            Execute(conn =>
            {
                using (SQLiteCommand cmd = conn.CreateCommand()) {
                    StringBuilder cmdBuffer = new StringBuilder("SELECT Name FROM ScheduledGrabs ");
                    cmd.CommandText = cmdBuffer.ToString();
                    cmd.ExecuteNonQuery();

                    SQLiteDataReader reader = cmd.ExecuteReader();
                    while (reader.Read()) {
                        try
                        {
                            string name = Convert.ToString(reader[0]);
                            jobNames.Add(name);
                        } catch (Exception ex) {
                            Console.WriteLine("Error loading job name:" + ex.Message);
                        }
                    }
                    reader.Close();
                }
            });
            List<ScheduledGrab> newGrabs = new List<ScheduledGrab>();
            foreach(var urlToGrab in urls)
            {
                if(string.IsNullOrWhiteSpace(urlToGrab))
                    continue;
                var clone = grabToDuplicate.Clone();
                //clone.GrabParams.Url = new GrabbableUrl(urlToGrab);
                clone.GrabParams = new GrabEventArgs(new GrabbableUrl(urlToGrab), string.Empty) { GrabExpression = grabToDuplicate.GrabParams.GrabExpression };
                var newName = Utils.GetNextAvailableName(clone.Name, jobNames);
                clone.Name = newName;
                jobNames.Add(newName);
                newGrabs.Add(clone);
                Console.WriteLine(clone.GrabParams.Url);
            }
            AddScheduledGrabs(newGrabs);
        }

        public List<ScheduledGrab> GetScheduledGrabs()
        {
            return GetScheduledGrabs(null);
        }

        public List<ScheduledGrab> GetScheduledGrabs(bool? onlyActive)
        {
            List<ScheduledGrab> jobs = new List<ScheduledGrab>();
            Execute(conn =>
            {
                using (SQLiteCommand cmd = conn.CreateCommand()) {
                    StringBuilder cmdBuffer = new StringBuilder("SELECT * FROM ScheduledGrabs ");

                    bool firstClause = true;
                    if (onlyActive.HasValue) {
                        cmdBuffer.Append(firstClause ? " WHERE" : " AND");
                        firstClause = false;
                        cmdBuffer.Append("  IsActive = @IsActive");
                        cmd.Parameters.AddWithValue("@IsActive", onlyActive.Value);
                    }
                    cmd.CommandText = cmdBuffer.ToString();
                    cmd.ExecuteNonQuery();

                    SQLiteDataReader reader = cmd.ExecuteReader();
                    while (reader.Read()) {
                        try {
                            ScheduledGrab grab = GetScheduledGrab(reader);
                            jobs.Add(grab);
                        } catch (Exception ex) {
                            Console.WriteLine("Error loading job:" + ex.Message);
                        }
                    }
                    reader.Close();
                }
            });
            return jobs;
        }

        private ScheduledGrab GetScheduledGrab(SQLiteDataReader reader)
        {
            ScheduledGrab grab = new ScheduledGrab()
            {
                GrabID = Convert.ToInt32(reader["GrabID"]),
                GrabParams = GrabEventArgs.Deserialize(Convert.ToString(reader["Parameters"])),
                GrabMode = (Constants.GrabModes)Enum.Parse(typeof(Constants.GrabModes), Convert.ToString(reader["Mode"]), true),
                GrabSchedule = (Constants.GrabSchedules)Enum.Parse(typeof(Constants.GrabSchedules), Convert.ToString(reader["Schedule"]), true),
                Interval = Convert.ToInt32(reader["Interval"]),
                IsActive = Convert.ToChar(reader["IsActive"]) == '1',
                Name = Convert.ToString(reader["Name"]),
            };
            return grab;
        }
        #endregion

        #region Grab History
        public void AddJobHistory(IEnumerable<GrabbedJob> grabbedJobs)
        {
            Execute(conn =>
            {
                using (SQLiteTransaction trans = conn.BeginTransaction()) {
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    using (SQLiteCommand cmdDisArm = conn.CreateCommand()) {
                        cmdDisArm.CommandText = "UPDATE ScheduledGrabs SET IsArmed = 'false' WHERE GrabID = @GrabID";
                        cmdDisArm.Parameters.AddWithValue("@GrabID", 0);
                        cmd.CommandText = "INSERT INTO GrabHistory (GrabID, Result, ResultCode, GrabDate) VALUES (@GrabID, @Result, @ResultCode, @GrabDate)";
                        cmd.Parameters.AddWithValue("@ItemID", 0);
                        cmd.Parameters.AddWithValue("@GrabID", string.Empty);
                        cmd.Parameters.AddWithValue("@Result", string.Empty);
                        cmd.Parameters.AddWithValue("@ResultCode", 0);
                        cmd.Parameters.AddWithValue("@GrabDate", DateTime.Now);

                        foreach (GrabbedJob grabbedJob in grabbedJobs) {
                            cmd.Parameters["@ItemID"].Value = grabbedJob.ItemID;
                            cmd.Parameters["@GrabID"].Value = grabbedJob.ScheduledGrab.GrabID;
                            cmd.Parameters["@Result"].Value = SerializationHelper.SerializeResponse(grabbedJob.Response.ParsedResponse);
                            cmd.Parameters["@ResultCode"].Value = grabbedJob.Result;
                            cmd.Parameters["@GrabDate"].Value = grabbedJob.GrabDate;
                            cmd.ExecuteNonQuery();

                            switch (grabbedJob.ScheduledGrab.GrabSchedule) {
                                case Constants.GrabSchedules.OneTime:
                                    cmdDisArm.Parameters["@GrabID"].Value = grabbedJob.ScheduledGrab.GrabID;// only set to run once, disarm it
                                    cmdDisArm.ExecuteNonQuery();
                                    break;
                            }
                        }
                    }
                    trans.Commit();
                }
            });
        }
        #endregion

        #region Templates
        public void AddGrabTemplates(IEnumerable<GrabTemplate> templates) {
            Execute(conn =>
            {
                using (SQLiteTransaction trans = conn.BeginTransaction()) {
                    using (SQLiteCommand cmd = conn.CreateCommand()) {
                        cmd.CommandText = "INSERT INTO GrabTemplates (TemplateID, Name, Description, UrlFormatString, GrabParameters, GrabMode) VALUES (@TemplateID, @Name, @Description, @UrlFormatString, @GrabParameters, @GrabMode)";
                        cmd.Parameters.AddWithValue("@TemplateID", string.Empty);
                        cmd.Parameters.AddWithValue("@Description", string.Empty);
                        cmd.Parameters.AddWithValue("@Name", string.Empty);
                        cmd.Parameters.AddWithValue("@UrlFormatString", string.Empty);
                        cmd.Parameters.AddWithValue("@GrabParameters", string.Empty);
                        cmd.Parameters.AddWithValue("@GrabMode", 0);

                        foreach (GrabTemplate template in templates) {
                            cmd.Parameters["@TemplateID"].Value = template.TemplateID;
                            cmd.Parameters["@Description"].Value = template.Description;
                            cmd.Parameters["@Name"].Value = template.Name;
                            cmd.Parameters["@UrlFormatString"].Value = template.UrlFormatString;
                            cmd.Parameters["@GrabParameters"].Value = template.GrabParameters.Serialize();
                            cmd.Parameters["@GrabMode"].Value = (int)template.GrabMode;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    trans.Commit();
                }
            });
        }

        public void EditGrabTemplate(GrabTemplate template) {
            Execute(conn =>
            {
                using (SQLiteCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = "UPDATE GrabTemplates SET Name = @Name, Description = @Description, UrlFormatString = @UrlFormatString, GrabParameters = @GrabParameters, GrabMode = @GrabMode WHERE TemplateID = @TemplateID";
                    cmd.Parameters.AddWithValue("@TemplateID", template.TemplateID);
                    cmd.Parameters.AddWithValue("@Description", template.Description);
                    cmd.Parameters.AddWithValue("@Name", template.Name);
                    cmd.Parameters.AddWithValue("@UrlFormatString", template.UrlFormatString);
                    cmd.Parameters.AddWithValue("@GrabParameters", template.GrabParameters.Serialize());
                    cmd.Parameters.AddWithValue("@GrabMode", (int)template.GrabMode);
                    cmd.ExecuteNonQuery();
                }
            });
        }

        public void DeleteGrabTemplate(GrabTemplate template) {
            Execute(conn =>
            {
                using (SQLiteCommand cmd = conn.CreateCommand()) {
                    cmd.Parameters.AddWithValue("@TemplateID", template.TemplateID);

                    cmd.CommandText = "DELETE FROM GrabTemplates WHERE TemplateID = @TemplateID";
                    cmd.ExecuteNonQuery();
                }
            });
        }

        public List<GrabTemplate> GetTemplates() {
            List<GrabTemplate> items = new List<GrabTemplate>();
            Execute(conn =>
            {
                using (SQLiteCommand cmd = conn.CreateCommand()) {
                    StringBuilder cmdBuffer = new StringBuilder("SELECT * FROM GrabTemplates ");
                    //bool firstClause = true;
                    //if (onlyActive.HasValue) {
                    //    cmdBuffer.Append(firstClause ? " WHERE" : " AND");
                    //    firstClause = false;
                    //    cmdBuffer.Append("  IsActive = @IsActive");
                    //    cmd.Parameters.AddWithValue("@IsActive", onlyActive.Value);
                    //}
                    cmd.CommandText = cmdBuffer.ToString();
                    cmd.ExecuteNonQuery();

                    SQLiteDataReader reader = cmd.ExecuteReader();
                    while (reader.Read()) {
                        try {
                            GrabTemplate template = GetGrabTemplate(reader);
                            items.Add(template);
                        } catch (Exception ex) {
                            Console.WriteLine("Error loading job:" + ex.Message);
                        }
                    }
                    reader.Close();
                }
            });
            return items;
        }

        private GrabTemplate GetGrabTemplate(SQLiteDataReader reader)
        {
            return new GrabTemplate()
                       {
                           Name = Convert.ToString(reader["Name"]),
                           Description = Convert.ToString(reader["Description"]),
                           TemplateID = Convert.ToString(reader["TemplateID"]),
                           UrlFormatString = Convert.ToString(reader["UrlFormatString"]),
                           GrabParameters = GrabEventArgs.Deserialize(Convert.ToString(reader["GrabParameters"])),
                           GrabMode = (Constants.GrabModes)Enum.Parse(typeof(Constants.GrabModes), Convert.ToString(reader["GrabMode"]), true),
                       };
        }

        #endregion

        #region Database Maintenance
        /// <summary>
        /// Creates a database and required tables.
        /// </summary>
        public override bool CreateDatabase()
        {
            bool created = false;
            FileInfo dbFile = new FileInfo(FileName);
            if(!dbFile.Directory.Exists) {
                if(!dbFile.Directory.Parent.Exists)
                    dbFile.Directory.Parent.Create();
                dbFile.Directory.Create();
            }
            SQLiteConnection.CreateFile(FileName);
            CreateTables();
            created = true;
            return created;
        }

        public void CreateTables()
        {
            Execute(conn =>
            {
                using (SQLiteCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = @"CREATE TABLE ScheduledGrabs (
							GrabID INTEGER PRIMARY KEY AUTOINCREMENT,
                            Interval TEXT(10) NOT NULL,
                            Name TEXT(255) NOT NULL,
                            IsActive TEXT,
                            Schedule INTEGER,
                            Mode INTEGER,
                            IsArmed INT DEFAULT 1,
                            Parameters TEXT
                        );
                        CREATE TABLE GrabHistory (
                            ItemID INTEGER PRIMARY KEY AUTOINCREMENT,
                            GrabID INTEGER NOT NULL,
                            Result TEXT NOT NULL COLLATE NOCASE,
                            ResultCode INTEGER NOT NULL,
                            GrabDate DATETIME
                        );
                        CREATE TABLE GrabTemplates (
                            TemplateID TEXT(10) PRIMARY KEY,
                            Name TEXT(255),
                            Description TEXT, 
                            UrlFormatString TEXT(255)  NOT NULL,
                            GrabParameters  TEXT,
                            GrabMode INTEGER NOT NULL
                        );
                        ";
                    cmd.ExecuteNonQuery();
                }
            });
        }
        #endregion
    }
}