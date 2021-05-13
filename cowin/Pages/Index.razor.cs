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

        }
        private async Task<IEnumerable<State>> SearchStates(string value)
        {
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
            if(this.SelectedState.state_id == 0)
            {
                _snackBar.Add("Select a State",Severity.Error);
                return;
            }
            if (this.SelectedDistrict.district_id == 0)
            {
                _snackBar.Add("Select a District", Severity.Error);
                return;
            }
            await FetchPublicReport(SelectedState.state_id, SelectedDistrict.district_id);
            var appointmentEndpoint = $"api/v2/appointment/sessions/public/calendarByDistrict?district_id={SelectedDistrict.district_id}&date={DateTime.Now.Date.ToString("dd/MM/yyyy")}";
            var response = await _httpClient.GetAsync(appointmentEndpoint);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Deserialize<CalendarByDistrictEndpointResponse>();               
                if(data.centers.Count == 0)
                {
                    _snackBar.Add("No Centers Found");
                }
                else
                {
                    var allCentres = data.centers.Count;
                    var temp = data.centers;
                    temp.RemoveAll(a => a.sessions.Any(a => a.available_capacity == 0));
                    if (temp.Count == 0)
                    {
                        centersFound = allCentres;
                        vaccinationSlotAvailable = false;
                    }
                    else
                    {
                        centersFound = temp.Count;
                        vaccinationSlotAvailable = true;
                    }
                   
                    Centers = temp;
                    searchingCompleted = true;
                }
            }
        }
        public Color GetFeeChipColor(string feeType)
        {
            if (feeType == "Free") return Color.Success;
            else return Color.Error;
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
