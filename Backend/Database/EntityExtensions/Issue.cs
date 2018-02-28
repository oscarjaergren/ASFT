using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTypes;

namespace Database
{
	public partial class Issue
	{
		[NotMapped]
		public IssueSeverity SeverityEnum
		{
			get
			{
				return (IssueSeverity) Severity;
			}
			set
			{
				if ( !Enum.IsDefined( typeof( IssueSeverity ), value ) )
					throw new ArgumentOutOfRangeException();
				Severity = (short)value;
			}
		}

		[NotMapped]
		public IssueStatus StatusEnum
		{
			get
			{
				return (IssueStatus)Status;
			}
			set
			{
				if ( !Enum.IsDefined( typeof( IssueStatus ), value ) )
					throw new ArgumentOutOfRangeException();
				Status = (short)value;
			}
		}
	}
}
