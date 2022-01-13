#region Copyright © 2008 Ashu Fouzdar. All rights reserved.
/*
Copyright © 2008 Ashu Fouzdar. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
3. The name of the author may not be used to endorse or promote products
   derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace aicl.SchemaGenerator
{
	/// <summary>
	/// Description of Schema.
	/// </summary>
	public class Schema
	{
		private string dbname=null;
		private string servername=null;
		private string username=null;
		private string password=null;
		private StringBuilder sb=null;
		
		private bool _DoSort=false;
		
		public bool DoSort {
			get { return _DoSort; }
			set { _DoSort = value; }
		}
		
		public Schema(string dbname)
		{
			this.servername=null;
			this.dbname=dbname;
		}

		public Schema(string servername, string dbname)
		{
			this.servername=servername;
			this.dbname=dbname;
		}
		
		public Schema(string servername, string dbname, string username, string password)
		{
			this.servername=servername;
			this.dbname=dbname;
			this.username=username;
			this.password=password;
		}

		public StringBuilder GetDBSchema()
		{
			string connectionString = GetConnectionString();
			this.sb=new StringBuilder();
			
			using (SqlConnection connection =
			       new SqlConnection(connectionString))
			{
				//Connect to the database, and then retrieve the
				//schema information.
				try {
					connection.Open();
				} catch (Exception ex) {
					System.Windows.Forms.MessageBox.Show("Error opening connection to database : " + ex.Message,"Error",System.Windows.Forms.MessageBoxButtons.OK,System.Windows.Forms.MessageBoxIcon.Warning);
					return null;
				}
				
				try {
					DataTable table = connection.GetSchema("Tables");
					sb.Append("================================================\n");
					sb.AppendFormat("Schema Description for  : {0} database ",this.dbname);
					sb.Append("\n================================================\n");
					
					if (_DoSort)
					{
						DataView dv=new DataView(table);
						dv.Sort=table.Columns[2].ColumnName + " ASC";
						table=dv.ToTable();
					}
					
					//table.Select(string.Empty,table.Columns[2].ColumnName + " ASC")
					foreach (System.Data.DataRow row in table.Rows)//table.Rows)
					{
						string[] restrictions = new string[4];
						restrictions[2] =(string) row[2];

						DataTable tblColumns = connection.GetSchema("Columns", restrictions);
						sb.AppendFormat("Table : {0}",row[2]);
						sb.Append("\n------------------------------------------------------------------------------------------------------------------------------\n");
						sb.AppendFormat("{0,-50}	{1,15}	{2,-3}	{3,-15}	{4,-20}","Column Name","Data Type","IsNull","Length ","Default Value");
						sb.Append("\n------------------------------------------------------------------------------------------------------------------------------\n");
						// Display the contents of the table.
						DisplayData(tblColumns);
						sb.Append("***********************************************************************************\n");
					}
					
				} catch (Exception ex) {
					System.Windows.Forms.MessageBox.Show("Error reading schema : " + ex.Message,"Error",System.Windows.Forms.MessageBoxButtons.OK,System.Windows.Forms.MessageBoxIcon.Warning);
					return null;
				}
				

			}
			return (sb);
		}

		private string GetConnectionString()
		{
			// To avoid storing the connection string in your code,
			// you can retrieve it from a configuration file.
			if (this.servername != null && this.username != null && this.password != null) {
				return string.Format("Data Source={0};Database={1};User ID={2};Password={3}",new object [] {this.servername,this.dbname,this.username,this.password});
			}
			else if (this.servername != null){
				return string.Format("Data Source={0};Database={1};Integrated Security=SSPI",this.servername,this.dbname);
			}
			else {
				return string.Format("Data Source=(local);Database={0};Integrated Security=SSPI",this.dbname);
			}
		}

		private void DisplayData(System.Data.DataTable table)
		{
			foreach (System.Data.DataRow row in table.Rows)
			{
				sb.AppendFormat("{0,-50}	{1,15}	{2,-3}	{3,-15}	{4,-20}\n",row[3],row[7],row[6],row[8],row[5]);
			}
		}

	}
}
