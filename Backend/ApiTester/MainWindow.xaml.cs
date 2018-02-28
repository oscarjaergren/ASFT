using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IssueManagerApiClient;
using ViewModels.Issue;
using ViewModels.Location;

namespace ApiTester
{
	public partial class MainWindow : Window
	{
		private const string Host = "http://localhost:52763/";
		IssueManagerClientApplication _applicationClient = new IssueManagerClientApplication( Host );
		IssueManagerClientUser _userClient = new IssueManagerClientUser( Host );

		public MainWindow()
		{
			InitializeComponent();
		}

		private void LogOnApplication_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				var vm = ShowInputWindow( new LoginInfo { UserName = "IssueManager", Password = "im2A1ge5" } );
				if ( vm != null )
				{
					_applicationClient.Login( vm.UserName, vm.Password );
					SetResultSuccess( _applicationClient.AccessToken );
					_userClient.LogOut();
				}
			}
			catch ( Exception ex )
			{
				SetResultError( "Application login failed with result: " + ex.GetType() );
			}
		}

		private void LogOutApplication_Click( object sender, RoutedEventArgs e )
		{
			_applicationClient.LogOut();
			SetResultSuccess( "Logged out" );
		}

		private void GetApplicationUsername_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				var userName = _applicationClient.GetUserName();
				SetResultSuccess( userName );
			}
			catch ( Exception ex )
			{
				SetResultError( "GetApplicationUsername failed with result: " + ex.GetType() );
			}
		}

		private void ImpersonateUser_Click( object sender, RoutedEventArgs e )
		{
			try
			{
        var vm = ShowInputWindow(new User { UserName = "LJU" /*"Piledal"*/ });
				if ( vm != null )
				{
					var token = _applicationClient.GetTokenForUser( vm.UserName );
					SetResultSuccess( token );

					_userClient = new IssueManagerClientUser( Host, token );
				}
			}
			catch ( Exception ex )
			{
				SetResultError( "GetTokenForUser failed with result: " + ex.GetType() );
			}
		}

		private void LogOn_Click( object sender, RoutedEventArgs e )
		{
			try
			{
        var vm = ShowInputWindow(new LoginInfo { UserName = "LJU" /*"Piledal"*/, Password = "4R5zE6mw" /*"70b1a5mL"*/});
				if ( vm != null )
				{
					_userClient.Login( vm.UserName, vm.Password );
					SetResultSuccess( _userClient.AccessToken );
				}
			}
			catch ( Exception ex )
			{
				SetResultError( "Login failed with result: " + ex.GetType() );
			}
		}

		private void LogOut_Click( object sender, RoutedEventArgs e )
		{
			_userClient.LogOut();
			SetResultSuccess( "Logged out" );
		}

		private void GetUserName_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				var userName = _userClient.GetUserName();
				SetResultSuccess( userName );
			}
			catch ( Exception ex )
			{
				SetResultError( "GetUserName failed with result: " + ex.GetType() );
			}
		}

		private void GetApiVersionUsed_Click( object sender, RoutedEventArgs e )
		{
			SetResultSuccess( "Api version used: " + _userClient.ApiVersionUsed );
		}

		private void ApiVersionUsedCompatible_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				var supported = _userClient.GetIsVersionSupported();
				SetResultSuccess( "Current api version supported: " + supported );
			}
			catch ( Exception ex )
			{
				SetResultError( "Checking for version 1 support failed with result: " + ex.GetType() );
			}
		}

		private void GetV1Supported_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				var supported = _userClient.GetIsVersionSupported( 1 );
				SetResultSuccess( "Version 1 supported: " + supported );
			}
			catch ( Exception ex )
			{
				SetResultError( "Checking for version 1 support failed with result: " + ex.GetType() );
			}
		}

		private void GetV2Supported_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				var supported = _userClient.GetIsVersionSupported( 2 );
				SetResultSuccess( "Version 2 supported: " + supported );
			}
			catch ( Exception ex )
			{
				SetResultError( "Checking for version 2 support failed with result: " + ex.GetType() );
			}
		}

		private void GetLocations_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				var locations = _userClient.GetLocations();
				SetResultSuccess( "Locations:\n" + string.Join( "\n", locations.Select( x => x.Id + " " + x.Name ) ) );
			}
			catch ( Exception ex )
			{
				SetResultError( "Get locations failed with result: " + ex.GetType() );
			}
		}

		private void GetUsersLocations_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				var locations = _userClient.GetUsersLocations();
				SetResultSuccess( "Locations:\n" + string.Join( "\n", locations.Select( x => x.Id + " " + x.Name ) ) );
			}
			catch ( Exception ex )
			{
				SetResultError( "Get locations failed with result: " + ex.GetType() );
			}
		}

		private void GetLocation_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				var vm = ShowInputWindow( new LocationId() );
				if ( vm != null )
				{
					var location = _userClient.GetLocation( vm.Id );

					SetResultSuccess( "Location was found!\n\n" + ObjectToString( location ) );
				}
			}
			catch ( Exception ex )
			{
				SetResultError( "Get location failed with result: " + ex.GetType() );
			}
		}

		private void NewLocation_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				var vm = ShowInputWindow( new NewLocationVM() );
				if ( vm != null )
				{
					var id = _userClient.CreateLocation( vm );
					SetResultSuccess( "Location created: " + id + " " + vm.Name );
				}
			}
			catch ( Exception ex )
			{
				SetResultError( "New location failed with result: " + ex.GetType() );
			}
		}

		private void UpdateLocation_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				var vm = ShowInputWindow( new LocationVM() );
				if ( vm != null )
				{
					_userClient.UpdateLocation( vm );
					SetResultSuccess( "Location updated: " + vm.Id + " " + vm.Name );
				}
			}
			catch ( Exception ex )
			{
				SetResultError( "Update location failed with result: " + ex.GetType() );
			}
		}

		private void DeleteLocation_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				var vm = ShowInputWindow( new LocationId() );
				if ( vm != null )
				{
					_userClient.DeleteLocation( vm.Id );
					SetResultSuccess( "Location deleted: " + vm.Id );
				}
			}
			catch ( Exception ex )
			{
				SetResultError( "Delete location failed with result: " + ex.GetType() );
			}
		}

		private void AllIssuesAtLocation_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				var vm = ShowInputWindow( new LocationId() );
				if ( vm != null )
				{
					var issues = _userClient.GetAllIssuesAtLocation( vm.Id );
					SetResultSuccess( "Issues at location " + vm.Id + ":\n" + string.Join( "\n", issues.Select( x => x.Id + " " + x.Status.ToString() + " " + x.Title ) ) );
				}
			}
			catch ( Exception ex )
			{
				SetResultError( "All issues at location failed with result: " + ex.GetType() );
			}
		}

		private void GetIssue_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				var vm = ShowInputWindow( new IssueId() );
				if ( vm != null )
				{
					var issue = _userClient.GetIssue( vm.Id );

					SetResultSuccess( "Issue was found!\n\n" + ObjectToString( issue ) );
				}
			}
			catch ( Exception ex )
			{
				SetResultError( "Get issue failed with result: " + ex.GetType() );
			}
		}

		private void NewIssue_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				var vm = ShowInputWindow( new NewIssueVM() );
				if ( vm != null )
				{
					var id = _userClient.CreateIssue( vm );
					SetResultSuccess( "Issue created: " + id + " " + vm.Title );
				}
			}
			catch ( Exception ex )
			{
				SetResultError( "New issue failed with result: " + ex.GetType() );
			}
		}

		private void UpdateIssue_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				var vm = ShowInputWindow( new IssueVM() );
				if ( vm != null )
				{
					_userClient.UpdateIssue( vm );
					SetResultSuccess( "Issue updated: " + vm.Id + " " + vm.Title );
				}
			}
			catch ( Exception ex )
			{
				SetResultError( "Update issue failed with result: " + ex.GetType() );
			}
		}

		private void UpdateIssueStatus_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				var vm = ShowInputWindow( new IssueStatusVM() );
				if ( vm != null )
				{
					_userClient.UpdateIssueStatus( vm );
					SetResultSuccess( "Issue status updated: " + vm.Id + " " + vm.Status );
				}
			}
			catch ( Exception ex )
			{
				SetResultError( "Update issue status failed with result: " + ex.GetType() );
			}
		}

		private void DeleteIssue_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				var vm = ShowInputWindow( new IssueId() );
				if ( vm != null )
				{
					_userClient.DeleteIssue( vm.Id );
					SetResultSuccess( "Issue deleted: " + vm.Id );
				}
			}
			catch ( Exception ex )
			{
				SetResultError( "Delete issue failed with result: " + ex.GetType() );
			}
		}

    private void GetImage_Click(object sender, RoutedEventArgs e)
		{
      try
      {
        var vm = ShowInputWindow(new IssueImageId());
        var result = _userClient.GetImage(vm.Id);

        SetResultSuccess(String.Format("Data recieved : {0} bytes recieved (Type : {1})", result.Item1.Length, result.Item2));
        //save image as file
      }
      catch (Exception ex)
      {
        SetResultError("GetImage failed with result: " + ex.GetType());
      }
    }

		private void UploadImage_Click( object sender, RoutedEventArgs e )
    {
      try
      {
        var vm = ShowInputWindow(new IssueId());

        Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
        dlg.FileName = "";
        dlg.DefaultExt = ".jpg";
        dlg.Filter = "Pictures (*.jpg)|*.jpg";
        bool? result = dlg.ShowDialog();
        if(result != null && result == true)
        {
          String fileType = "";
          String filename = dlg.FileName;
          int pos = filename.LastIndexOf('.');
          if(pos > 0)
          {
            fileType = filename.Substring(pos+1);
            fileType = fileType.ToLower();
          }
          
          // var bytes = File.ReadAllBytes( @"C:\IssueManager\Photos\Untitled.png" );
          var bytes = File.ReadAllBytes(dlg.FileName);

          _userClient.UploadImage(vm.Id, bytes, fileType);

          SetResultSuccess("Upload image: Success");
        }
      }
      catch (Exception ex)
      {
        SetResultError("Upload image failed with result: " + ex.GetType());
      }
    }

    private void GetAllImagesForIssue_Click(object sender, RoutedEventArgs e)
    {
      try 
      {
        var vm = ShowInputWindow(new IssueId());
        var images = _userClient.GetImages(vm.Id);
        SetResultSuccess("Images for issue " + vm.Id + ":\n" + string.Join("\n", images.Select(x => x.Id + " " + x.Image.FileName + " " + x.IssueId)));

      }
      catch(Exception ex)
      {
        SetResultError("GetAllImagesForIssue failed with result: " + ex.GetType());
      }
    }

		private void SetResultSuccess( string text )
		{
			tbResult.Text = text;
			tbResult.Foreground = Brushes.Black;
		}

		private void SetResultError( string text )
		{
			tbResult.Text = text;
			tbResult.Foreground = Brushes.Red;
		}

		private T ShowInputWindow<T>( T model ) where T : class
		{
			var window = new InputWindow( model );
			var result = window.ShowDialog();

			if ( result == true )
				return model;
			return null;
		}

		private string ObjectToString( object obj )
		{
			var objType = obj.GetType();
			return string.Join( "\n", objType.GetProperties().Select( property => property.Name + ": " + property.GetValue( obj ) ) );
			//var objString = "";
			//foreach ( var property in obj.GetType().GetProperties() )
			//{
			//	objString += "\n" + property.Name + ": " + property.GetValue( obj );
			//}
			//return objString;
		}

  
	}

	public class LoginInfo
	{
		public string UserName { get; set; }
		public string Password { get; set; }
	}

	public class User
	{
		public string UserName { get; set; }
	}

	public class LocationId
	{
		public int Id { get; set; }
	}

	public class IssueId
	{
		public int Id { get; set; }
	}

	public class IssueImageId
	{
		public int Id { get; set; }
	}
}
