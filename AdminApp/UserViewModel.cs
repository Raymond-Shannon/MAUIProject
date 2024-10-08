using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace AdminApp
{
		[FirestoreData]
		public class UserModel
		{
				[FirestoreProperty]
				public string Email { get; set; }

				[FirestoreProperty]
				public bool IsFirstLogin { get; set; }

				[FirestoreProperty]
				public string Token { get; set; }

				[FirestoreProperty]
				public string Username { get; set; }

				[FirestoreProperty]
				public DateTime CreatedAt { get; set; }
		}
}

