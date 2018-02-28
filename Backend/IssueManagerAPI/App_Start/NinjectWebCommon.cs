using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Mvc;
using Authentication;
using DataAccess.Repositories.Implementations;
using DataAccess.Repositories.Interfaces;
using MembershipProviderAuthentication;
using Ninject.Syntax;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(IssueManagerAPI.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(IssueManagerAPI.App_Start.NinjectWebCommon), "Stop")]

namespace IssueManagerAPI.App_Start
{
	using System;
	using System.Web;

	using Microsoft.Web.Infrastructure.DynamicModuleHelper;

	using Ninject;
	using Ninject.Web.Common;

	public static class NinjectWebCommon 
	{
		private static readonly Bootstrapper bootstrapper = new Bootstrapper();
		public static IKernel Kernel { get; private set; }

		/// <summary>
		/// Starts the application
		/// </summary>
		public static void Start() 
		{
			DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
			DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
			bootstrapper.Initialize(CreateKernel);
		}
		
		/// <summary>
		/// Stops the application.
		/// </summary>
		public static void Stop()
		{
			bootstrapper.ShutDown();
		}
		
		/// <summary>
		/// Creates the kernel that will manage your application.
		/// </summary>
		/// <returns>The created kernel.</returns>
		private static IKernel CreateKernel()
		{
			Kernel = new StandardKernel();
			try
			{
				Kernel.Bind<Func<IKernel>>().ToMethod( ctx => () => new Bootstrapper().Kernel );
				Kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

				RegisterServices();

				var ninjectDepencyResolver = new NinjectDependencyResolver( Kernel );
				DependencyResolver.SetResolver( ninjectDepencyResolver );
				GlobalConfiguration.Configuration.DependencyResolver = ninjectDepencyResolver;

				return Kernel;
			}
			catch
			{
				Kernel.Dispose();
				Kernel = null;
				throw;
			}
		}

		/// <summary>
		/// Load your modules or register your services here!
		/// </summary>
		/// <param name="kernel">The kernel.</param>
		private static void RegisterServices()
		{
			Kernel.Bind<IAuthentication>().To<MembershipAuthentication>();

			Kernel.Bind<IIssueImageRepository>().To<IssueImageRepository>();
			Kernel.Bind<IIssueRepository>().To<IssueRepository>();
			Kernel.Bind<ILocationRepository>().To<LocationRepository>();
		}
	}

	public class NinjectDependencyResolver : System.Web.Mvc.IDependencyResolver, System.Web.Http.Dependencies.IDependencyResolver
	{
		private IKernel kernel;

		public NinjectDependencyResolver( IKernel kernel )
		{
			this.kernel = kernel;
		}

		public object GetService( Type serviceType )
		{
			return this.kernel.TryGet( serviceType );
		}

		public IEnumerable<object> GetServices( Type serviceType )
		{
			try
			{
				return this.kernel.GetAll( serviceType );
			}
			catch ( Exception )
			{
				return new List<object>();
			}
		}

		public IDependencyScope BeginScope()
		{
			return new NinjectDependencyScope( kernel.BeginBlock() );
		}

		public void Dispose()
		{
			var disposable = kernel as IDisposable;
			if ( disposable != null )
				disposable.Dispose();

			kernel = null;
		}
	}

	public class NinjectDependencyScope : IDependencyScope
	{
		IResolutionRoot resolver;

		public NinjectDependencyScope( IResolutionRoot resolver )
		{
			this.resolver = resolver;
		}

		public object GetService( Type serviceType )
		{
			if ( resolver == null )
				throw new ObjectDisposedException( "this", "This scope has been disposed" );

			return resolver.TryGet( serviceType );
		}

		public System.Collections.Generic.IEnumerable<object> GetServices( Type serviceType )
		{
			if ( resolver == null )
				throw new ObjectDisposedException( "this", "This scope has been disposed" );

			return resolver.GetAll( serviceType );
		}

		public void Dispose()
		{
			var disposable = resolver as IDisposable;
			if ( disposable != null )
				disposable.Dispose();

			resolver = null;
		}
	}
}
