﻿using Autofac;
using Bit.CSharpClientSample.Data;
using Bit.CSharpClientSample.ViewModels;
using Bit.CSharpClientSample.Views;
using Bit.Model.Events;
using Bit.Tests.Model.Dto;
using Bit.ViewModel.Contracts;
using Bit.ViewModel.Implementations;
using Prism;
using Prism.Autofac;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Bit.CSharpClientSample
{
    public partial class App : BitApplication
    {
        public App(IPlatformInitializer initializer)
                    : base(initializer)
        {

        }

        protected override async Task OnInitializedAsync()
        {
            InitializeComponent();

            bool isLoggedIn = await Container.Resolve<ISecurityService>().IsLoggedInAsync();

            if (isLoggedIn)
            {
                await NavigationService.NavigateAsync("/Nav/Main");
            }
            else
            {
                await NavigationService.NavigateAsync("/Login");
            }

            IEventAggregator eventAggregator = Container.Resolve<IEventAggregator>();

            eventAggregator.GetEvent<TokenExpiredEvent>()
                .SubscribeAsync(async tokenExpiredEvent => await NavigationService.NavigateAsync("/Login"), ThreadOption.UIThread);

            await base.OnInitializedAsync();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            ContainerBuilder containerBuilder = containerRegistry.GetBuilder();

            containerRegistry.RegisterForNav<NavigationPage>("Nav");

            containerRegistry.RegisterForNav<LoginView, LoginViewModel>("Login");
            containerRegistry.RegisterForNav<MainView, MainViewModel>("Main");
            containerRegistry.RegisterForNav<TestView, TestViewModel>("Test");

            containerBuilder.Register<IClientAppProfile>(c => new DefaultClientAppProfile
            {
                HostUri = new Uri("http://192.168.1.28/"),
                //HostUri = new Uri("http://127.0.0.1/"),
                //HostUri = new Uri("http://10.0.2.2"),
                OAuthRedirectUri = new Uri("Test://oauth2redirect"),
                AppName = "Test",
                ODataRoute = "odata/Test/"
            }).SingleInstance();

            containerBuilder.RegisterRequiredServices();
            containerBuilder.RegisterHttpClient();
            containerBuilder.RegisterODataClient();
            containerBuilder.RegisterIdentityClient();

            containerBuilder.RegisterDbContext<SampleDbContext>();

            containerBuilder.RegisterDefaultSyncService(syncService =>
            {
                syncService.AddDtoSetSyncConfig(new DtoSetSyncConfig
                {
                    DtoSetName = nameof(SampleDbContext.TestCustomers),
                    OnlineDtoSet = odataClient => odataClient.For(nameof(SampleDbContext.TestCustomers)),
                    OfflineDtoSet = dbContext => dbContext.Set<TestCustomerDto>()
                });
            });

            base.RegisterTypes(containerRegistry);
        }
    }
}
