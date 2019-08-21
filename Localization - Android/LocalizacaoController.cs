using CoreLocation;
using ForcaDeVendasMobile.Dependencias;
using ForcaDeVendasMobile.Model.DAL;
using ForcaDeVendasMobile.Model.Entities;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ForcaDeVendasMobile.Controller
{
    public class LocalizacaoController
    {

        public static bool IsRunning;
        public static DateTime UltimaLocalizacao;


        public static async void ObterLocalizacao(bool restart = false)
        {
            try
            {
                ConfiguracaoGps config = AcessoDados.FindAll<ConfiguracaoGps>(childrens: false).FirstOrDefault();

                if (CrossGeolocator.Current.IsGeolocationAvailable && config.Frequencia > 0 && DateTime.Now.Hour > config.HoraInicio &&
                     DateTime.Now.Hour < config.HoraFim)
                {                    

                    var locator = CrossGeolocator.Current;
                   
                    locator.DesiredAccuracy = 50;
                    if (!locator.IsListening || restart)
                        if (restart) { await locator.StopListeningAsync(); }
                        await locator.StartListeningAsync(minimumTime: TimeSpan.FromSeconds(config.Frequencia * 60), minimumDistance: config.Distancia, includeHeading: true,
                            listenerSettings: new Plugin.Geolocator.Abstractions.ListenerSettings
                            {
                                ActivityType = Plugin.Geolocator.Abstractions.ActivityType.AutomotiveNavigation,
                                AllowBackgroundUpdates = true,
                                DeferralDistanceMeters = 1,
                                DeferralTime = TimeSpan.FromSeconds(1),
                                ListenForSignificantChanges = true,
                                PauseLocationUpdatesAutomatically = false
                            }
                        );

                    CrossGeolocator.Current.PositionChanged += Current_PositionChanged;
                }



            }
            catch (Exception)
            {

                throw;
            }
        }

        private static async void Current_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            try
            {
                if (CrossGeolocator.Current.IsGeolocationEnabled)
                {
                    ConfiguracaoGps config = AcessoDados.FindAll<ConfiguracaoGps>(childrens: false).FirstOrDefault();
                    var location = CrossGeolocator.Current;

                    if (!config.LocalizacaoVendedor)
                    {
                        Position posicao = null;
                        List<ClienteLocalizacao> clientes = AcessoDados.FindAll<ClienteLocalizacao>(childrens: false);
                        foreach (var item in clientes)
                        {

                            double distancia = await DistanciaEntreCordenadasGeograficas(Math.Round(e.Position.Latitude, 7), Math.Round(e.Position.Longitude, 7),
                                                                                        Math.Round(item.Latitude, 7), Math.Round(item.Longitude, 7));

                            config = AcessoDados.FindAll<ConfiguracaoGps>(childrens: false).FirstOrDefault();

                            if (distancia <= config.DistanciaParaCheckin)
                            {
                                if (item.ControleChekin.Date < DateTime.Now.Date)
                                {
                                    item.ControleChekin = DateTime.Now;
                                    posicao = new Position(item.Latitude, item.Longitude);
                                    item.Save(true, false, false);
                                    item.Cliente = AcessoDados.Find<Cliente>(x => x.Oid == item.OidCliente, childrens: false);
                                    await FazerChekinCliente(item.Cliente, posicao);
                                }
                            }


                        }

                    }
                    else
                    {


                        //var address = await location.GetAddressesForPositionAsync(e.Position);
                        //string endereco = "";

                        //foreach (var item in address)
                        //{
                        //    endereco += item.CountryName + "+";
                        //    endereco += item.Locality + "+";
                        //    endereco += item.SubLocality + "+";
                        //    endereco += item.Thoroughfare + "+";
                        //    endereco += item.FeatureName + "+";
                        //    endereco += item.PostalCode;
                        //    break;
                        //}
                        //local.Endereco = endereco;
                        if (UltimaLocalizacao < (DateTime.Now.AddMinutes(-config.Frequencia)))
                        {
                         UltimaLocalizacao = DateTime.Now;
                       
                        Localizacao local = new Localizacao();
                        local.Oid = Guid.NewGuid();
                        local.Vendedor = AcessoDados.FindAll<Vendedor>().FirstOrDefault().Oid;

                        local.Latitude = Math.Round(e.Position.Latitude, 7);
                        local.Longitude = Math.Round(e.Position.Longitude, 7);
                        local.Ativo = true;
                        local.DataAtualizacao = Sincroniza.FormatDefaultDate(DateTime.Now);

                        if (Utils.Util.Conectado())
                            await SincronizarController.SincronizarLocalizacao(local);
                        else
                            local.Save(true, false, false);

                        }
                    }
                }

            }
            catch (Exception r)
            {

                throw;
            }
        }

        public static async Task<bool> FazerChekinCliente(Cliente cliente, Position posicao = null)
        {
            try
            {

                if (GpsHabilitado())
                {
                    Utils.Util.Mensagem = "";
                    Localizacao local = new Localizacao();

                    local.Oid = Guid.NewGuid();
                    var location = CrossGeolocator.Current;
                    location.DesiredAccuracy = 120;
                    if (posicao == null)
                    {                        
                        ClienteLocalizacao clienteLocalizacao = AcessoDados.Find<ClienteLocalizacao>(x => x.OidCliente == cliente.Oid);                       
                        if(clienteLocalizacao != null && clienteLocalizacao.ControleChekin < DateTime.Now.AddMinutes(-5))
                        {
                            posicao = await location.GetPositionAsync(timeout: TimeSpan.FromSeconds(120)); 
                            clienteLocalizacao.ControleChekin = DateTime.Now;
                            clienteLocalizacao.Save(true, false, false);
                            IsRunning = false;                           
                        }
                        else
                        {                             
                            IsRunning = false;
                            return true;
                        }
                    }
                    local.Vendedor = AcessoDados.FindAll<Vendedor>().FirstOrDefault().Oid;
                    local.Latitude = Math.Round(posicao.Latitude, 7);
                    local.Longitude = Math.Round(posicao.Longitude, 7);
                    local.Ativo = true;
                    local.DataAtualizacao = Sincroniza.FormatDefaultDate(DateTime.Now);
                    local.Cliente = cliente.Oid;

                    if (Utils.Util.Conectado())
                    {                        

                        await SincronizarController.SincronizarLocalizacao(local);
                    }
                    else
                        local.Save(true, false, false);
                }
                else
                {
                    IsRunning = false;
                    Utils.Util.Mensagem = "Habilite o serviço de Localização no seu celular.";
                    return false;
                }
                IsRunning = false;
                Utils.Util.Mensagem = "Agenda Atualizada com sucesso.";
                return true;
            }
            catch (Exception n)
            {
                IsRunning = false;
                Utils.Util.Mensagem = "Agenda não foi atualizada. Motivo: " + n.Message;
                return false;
            }
            finally
            {
                IsRunning = false;
            }
        }

        public static List<Localizacao> UpdateAdress(List<Localizacao> locais)
        {

            try
            {
                foreach (var it in locais)
                {
                    var location = CrossGeolocator.Current;

                    IEnumerable<Address> address = null;

                    Position position = new Position() { Latitude = it.Latitude, Longitude = it.Longitude };

                    Task.Run(async () => address = await location.GetAddressesForPositionAsync(position)).Wait();
                    string endereco = "";

                    if (address != null)
                    {
                        foreach (var item in address)
                        {
                            endereco += item.CountryName + "+";
                            endereco += item.Locality + "+";
                            endereco += item.SubLocality + "+";
                            endereco += item.Thoroughfare + "+";
                            endereco += item.FeatureName + "+";
                            endereco += item.PostalCode;
                            break;
                        }
                        it.Endereco = endereco;
                    }
                }
                return locais;
            }
            catch (Exception e)
            {

                return null;
            }
        }

        public static async void UpdateClienteAddreess()
        {
            try
            {
                List<Cliente> clientes = AcessoDados.FindAll<Cliente>(false);

                foreach (var cliente in clientes)
                {
                    IsRunning = true;

                    ClienteLocalizacao clienteLocal = AcessoDados.Find<ClienteLocalizacao>(x => x.OidCliente == cliente.Oid, childrens: false);


                    if (clienteLocal == null)
                    {
                        var location = CrossGeolocator.Current;

                        var postiion = (await location.GetPositionsForAddressAsync(Endereco(cliente), mapKey: null)).FirstOrDefault();
                        if (postiion != null)
                        {
                            ClienteLocalizacao lc = new ClienteLocalizacao();
                            lc.Oid = Guid.NewGuid();
                            lc.OidCliente = cliente.Oid;
                            lc.Latitude = Math.Round(postiion.Latitude, 8);//UtilidadesAIS.Utils.TruncateDouble(postiion.Latitude, 8);
                            lc.Longitude = postiion.Longitude;
                            lc.DataAtualizacao = cliente.DataAtualizacao;
                            lc.ControleChekin = DateTime.MinValue;
                            lc.Ativo = cliente.Ativo;

                            lc.Save(usarTransaction: true, validar: false, withChildren: false);
                             
                        }
                    }
                    else
                    {
                        DateTime dataCliente = Convert.ToDateTime(cliente.DataAtualizacao);
                        DateTime dataLocalizacao = Convert.ToDateTime(clienteLocal.DataAtualizacao);
                        if (dataCliente > dataLocalizacao)
                        {
                            var location = CrossGeolocator.Current;

                            var postiion = (await location.GetPositionsForAddressAsync(Endereco(cliente), mapKey: null)).FirstOrDefault();
                            if (postiion != null)
                            {
                                clienteLocal.Latitude = Math.Round(postiion.Latitude, 7);
                                clienteLocal.Longitude = Math.Round(postiion.Longitude, 7);
                                clienteLocal.DataAtualizacao = cliente.DataAtualizacao;
                                clienteLocal.Save(usarTransaction: true, validar: false, withChildren: false);
                            }
                        }
                    }
                }

                IsRunning = false;
            }
            catch (Exception e)
            {
                IsRunning = false;
                throw;
            }
            finally
            {
                IsRunning = false;
            }
        }


        protected static string Endereco(Cliente cliente)
        {
            return cliente.Endereco + " " + cliente.Cidade + " " + cliente.Uf;
        }

        public static async Task<double> DistanciaEntreCordenadasGeograficas(double LatitudeA, double LongitudeA, double LatitudeB, double LongitudeB)
        {
            double r = 6371.0;

            LatitudeA = LatitudeA * Math.PI / 180;
            LongitudeA = LongitudeA * Math.PI / 180;
            LatitudeB = LatitudeB * Math.PI / 180;
            LongitudeB = LongitudeB * Math.PI / 180;

            double dLat = LatitudeB - LatitudeA;
            double dLong = LongitudeB - LongitudeA;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(LatitudeA) * Math.Cos(LatitudeB) * Math.Sin(dLong / 2) * Math.Sin(dLong / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return Math.Round(r * c * 1000);
        }

        public static bool GpsHabilitado()
        {            
            if (!CrossGeolocator.IsSupported)
                return false;
            if (!CrossGeolocator.Current.IsGeolocationAvailable || !CrossGeolocator.Current.IsGeolocationEnabled)
                return false;
            return true;
        }

        #region RequestLocationPermission
        /// <summary>
        /// Verifica se há permissão de localização, se false solicita nova permissão.
        /// </summary>
        /// <returns></returns>
        public static bool RequestLocationPermission()
        {
            try
            {
                DependencyService.Register<IPermission>();
                DependencyService.Get<IPermission>().CheckLocationPermission();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        } 
        #endregion
    }
}
