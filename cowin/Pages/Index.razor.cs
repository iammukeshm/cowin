using cowin.Extensions;
using cowin.Models;
using cowin.Responses;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cowin.Pages
{
    public partial class Index
    {
        [Parameter]
        public State SelectedState { get; set; } = new();
        [Parameter]
        public District SelectedDistrict { get; set; } = new();
        [Parameter]
        public int? ZipCode { get; set; } = null;
        public List<State> States { get; set; } = new();
        public List<District> Districts { get; set; } = new();
        [Parameter]
        public List<Center> Centers { get; set; } = new();
        DateTime? date = DateTime.Now.Date;
        [Parameter]
        public bool vaccinationSlotAvailable { get; set; } = false;
        [Parameter]
        public bool searchingCompleted { get; set; } = false;
        [Parameter]
        public TopBlock PublicReportData { get; set; } = new();
        public int centersFound;
        protected override async Task OnInitializedAsync()
        {
            await FetchPublicReport();
            await FetchStates();
        }
        private async Task ResetAsync()
        {
            SelectedState = new();
            Districts = new();
            SelectedDistrict = new();
            date = DateTime.Now.Date;
            searchingCompleted = false;
            await FetchPublicReport();
            Centers = new();
            ZipCode = null;

        }
        private async Task<IEnumerable<State>> SearchStates(string value)
        {
            await ResetAsync();
            if (string.IsNullOrEmpty(value))
                return States;
            return States.Where(x => x.state_name.Contains(value, StringComparison.InvariantCultureIgnoreCase));
        }
        private async Task<IEnumerable<District>> SearchDistricts(string value)
        {
            await FetchDistricts();
            if (string.IsNullOrEmpty(value))
                return Districts;
            return Districts.Where(x => x.district_name.Contains(value, StringComparison.InvariantCultureIgnoreCase));
        }
        private async Task FetchStates()
        {
            States = new();
            SelectedState = new();
            var response = await _httpClient.GetAsync("api/v2/admin/location/states");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Deserialize<StateEndpointResponse>();
                States = data.states;
            }
        }
        private async Task FetchDistricts()
        {
            Districts = new();
            SelectedDistrict = new();
            var response = await _httpClient.GetAsync($"api/v2/admin/location/districts/{SelectedState.state_id}");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Deserialize<DistrictEndpointResponse>();
                Districts = data.districts;
            }
        }
        private async Task SearchForAvailableVaccinesAsync()
        {
            if (this.SelectedState.state_id == 0)
            {
                _snackBar.Add("Select a State", Severity.Error);
                return;
            }
            if (this.SelectedDistrict.district_id == 0)
            {
                _snackBar.Add("Select a District", Severity.Error);
                return;
            }
            if (date < DateTime.Now.Date)
            {
                _snackBar.Add("Please select Today or Later dates");
                searchingCompleted = false;
                Centers.Clear();
            }
            else
            {
                await FetchPublicReport(SelectedState.state_id, SelectedDistrict.district_id);
                string customDate = date.HasValue ? date.Value.ToString("dd-MM-yyyy") : string.Empty;
                var appointmentEndpoint = $"api/v2/appointment/sessions/public/calendarByDistrict?district_id={SelectedDistrict.district_id}&date={customDate}";
                var response = await _httpClient.GetAsync(appointmentEndpoint);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Deserialize<CalendarByDistrictEndpointResponse>();
                    if (data.centers.Count == 0)
                    {
                        _snackBar.Add("No Centers Found");
                        searchingCompleted = false;
                        Centers.Clear();
                    }
                    else
                    {
                        var allCentres = data.centers.Count;
                        var centers = data.centers;
                        SetCenters(allCentres, centers);
                    }
                    _snackBar.Add("Search result is from " + date.Value.ToString("dd-MM-yyyy") + " to " + date.Value.AddDays(6).ToString("dd-MM-yyyy"), Severity.Info);
                }
            }
        }
        private async Task SearchForAvailableVaccinesZipAsync()
        {
            if (ZipCode == null)
            {
                _snackBar.Add("please enter a ZIP code", Severity.Error);
                return;
            }
            else if(this.ZipCode.ToString().Length > 6 && this.ZipCode.ToString().Length == 0)
            {
                _snackBar.Add("Enter a valid ZIP code", Severity.Error);
                return;
            }
            else
            {
                await FetchPublicReport(SelectedState.state_id, SelectedDistrict.district_id);
                string customDate = date.HasValue ? date.Value.ToString("dd-MM-yyyy") : string.Empty;
                var appointmentEndpoint = $"api/v2/appointment/sessions/public/calendarByPin?pincode={ZipCode}&date={customDate}";
                var response = await _httpClient.GetAsync(appointmentEndpoint);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Deserialize<CalendarByDistrictEndpointResponse>();
                    if (data.centers.Count == 0)
                    {
                        _snackBar.Add("No Centers Found");
                        searchingCompleted = false;
                        Centers.Clear();
                    }
                    else
                    {
                        var allCentres = data.centers.Count;
                        var centers = data.centers;
                        SetCenters(allCentres, centers);
                    }
                    _snackBar.Add("Search result is from " + date.Value.ToString("dd-MM-yyyy") + " to " + date.Value.AddDays(6).ToString("dd-MM-yyyy"), Severity.Info);
                }
            }
        }
        public void SetCenters(int allCentres,List<Center> centers)
        {
            //temp.RemoveAll(a => a.sessions.Any(a => a.available_capacity == 0));
            foreach (Center center in centers.ToList())
            {
                //center.sessions.RemoveAll(x => !(x.date.Equals(customDate)));
                if (center.sessions.Sum(a => a.available_capacity) == 0)
                    centers.Remove(center);
            }
            if (centers.Count == 0)
            {
                centersFound = allCentres;
                vaccinationSlotAvailable = false;
            }
            else
            {
                centersFound = centers.Count;
                vaccinationSlotAvailable = true;
            }

            Centers = centers;
            searchingCompleted = true;
        }
        public Color GetFeeChipColor(string feeType)
        {
            if (feeType == "Free") { return Color.Success; }
            else if (feeType == "Paid") { return Color.Error; }
            else { return Color.Error; }
        }
        public async Task FetchPublicReport(int stateId = 0, int districtId = 0)
        {
           var publicReportEndpoint = $"api/v1/reports/v2/getPublicReports?state_id={(stateId > 0 ? stateId : string.Empty)}&district_id={(districtId > 0 ? districtId : string.Empty)}&date=";
           var response = await _httpClient.GetAsync(publicReportEndpoint);
           if (response.IsSuccessStatusCode)
           {
                    var data = await response.Deserialize<PublicReportEndpointResponse>();
                    PublicReportData = data.topBlock;
           }
        }
    }
}
