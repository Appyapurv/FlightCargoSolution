var app;
var cityLocations = {};

function getSelected() {
  rval = {};
  if (app == null) return rval;

  if (app.selectedType == "plane") {
    app.planes.forEach((plane) => {
      if (plane.callsign == app.selected) {
        rval = plane;
      }
    });
  } else {
    app.cities.forEach((city) => {
      if (city.name == app.selected) {
        rval = city;
      }
    });
  }
  return rval;
}

/* Update this when we click on it*/
async function cityClicked(city) {
  app.selected = city.name;

  c = await getCargo(city.name);
  if (c != null) {
    city.cargo = c;
  }
  app.selectedType = "city";
}

async function acceptCargo() {
  if (app.destinations == "") return;
  try {
    selectedCity = null;

    app.cities.forEach((city) => {
      if (city.name == app.selected) {
        selectedCity = city;
      }
    });
    if (selectedCity) {
      await newCargo(selectedCity.name, app.destinations);
      await cityClicked(selectedCity);
    }
  } catch (error) {
    console.error(error);
  }
}

async function planeClicked(plane) {
  app.selected = plane.callsign;
  app.selectedType = "plane";
}

async function changeCourier(cargo) {
  var courier = app.cargoCourier[cargo];
  await assignCourier(cargo, courier);
  //Update cargo locally
  app.cities.forEach((city) => {
    if (city.name == app.selected) {
      cityClicked(city);
    }
  });
}

async function onLoad() {
  app = new Vue({
    el: "#app",
    comments: true,
    data: {
      selected: null,
      selectedType: null,
      destinations: "",
      cities: [],
      planes: [],
      cargoCourier: {},
    },
    methods: {
      cityClicked: cityClicked,
      planeClicked: planeClicked,
      addDestination: addDestination,
      replaceRoute: replaceRoute,
      getSelected: getSelected,
      acceptCargo: acceptCargo,
      changeCourier: changeCourier,
    },
  });
  await GetCities();
  GetPlanes();
  setInterval(GetPlanes, 3);
}
