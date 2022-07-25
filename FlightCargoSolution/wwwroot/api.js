async function GetCities()
{ 
  try {
    const response = await axios.get('/cities');
    app.cities = response.data;
  
    /* Index by id */
    app.cities.forEach(city => {
      cityLocations[city.name] = city.location
    });
  } catch (error) {
    console.error(error);
  }
}
 
async function GetPlanes()
{ 
  try {
    const response = await axios.get('/planes');
    //If we have one selected we need cargo data
    if (app.selected != null && app.selectedType == "plane"){
        selectedPlane = null;
        response.data.forEach( p => {
            if(p.callsign == app.selected) {
                selectedPlane = p 
            }
        })
        if(selectedPlane) { 
            c =  await getCargo(selectedPlane.callsign)
            if(c != null) {
                selectedPlane.cargo = c
            }
            app.planes = response.data; 
            
        } else {
            app.planes = response.data;
        }
    } else {
        app.planes = response.data
    }
  } catch (error) {
    console.error(error);
  }
}



async function addDestination(plane) {
    if(app.destinations == "" ) return;
        try {
       const response = await axios.post(`/planes/${plane}/route/${app.destinations}`);
     } catch (error) {
       console.error(error);
     }
  }


  async function newCargo(from,to) {
    if(app.destinations == "" ) return;
        try {
       const response = await axios.post(`/cargo/${from}/to/${to}`);
     } catch (error) {
       console.error(error);
     }
  }

 
  async function replaceRoute(plane) {
   if(app.destinations == "" ) return;
     try {
       const response = await axios.put(`/planes/${plane}/route/${app.destinations}`);
     } catch (error) {
       console.error(error);
     }
  }

  async function getCargo(location) {
    try {
        const response = await axios.get(`/cargo/location/${location}`);
        return response.data
      } catch (error) {
        console.error(error);
      }
  }


 async function assignCourier(cargo,courier) {
    try {
        const response = await axios.put(`/cargo/${cargo}/courier/${courier}`);
        return response.data
      } catch (error) {
        console.error(error);
      }
  }
