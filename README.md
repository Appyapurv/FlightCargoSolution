**FlightCargoSolution**


**Index -**
1. created index on cities& planes for GeoSphere 2D
2. Index created will be handle in API solution

Developed and tested solution using MongoDB Atlas 

**Schema Changes
**
1. Added schema version for cargo collection to maintain the schema versioning
2. Added few field in plane collection to maintain the distancetravelled, timeTravelled, ManintaianceRequired
3. Added changestream for methods
4. GeoNear is used to calculate the distanace travelled.
5. 

**Logging** 
1. Default logging is applied through out the code.
2. logging the custom message

**Testing-**
Tested solution with 200 planes and 2000+ cities


**Index Added On- **

//Create 2d shpere index for position in cities
db.cities.createIndex( { position: "2dsphere" } )

//Create index for cargo location
db.cargo.createIndex( { location: 1 } )

//Create 2d shpere index for position in cities
db.plane.createIndex({ currentLocation:"2dsphere"})



aggregation Pipeline- 


db.worldcities.aggregate([{
 $match: {
  population: {
   $gt: '1000'
  }
 }
}, {
 $sort: {
  population: -1
 }
}, {
 $group: {
  _id: '$country',
  cities: {
   $push: '$$ROOT'
  }
 }
}, {
 $project: {
  _id: '$_id',
  cities15: {
   $slice: [
    '$cities',
    15
   ]
  }
 }
}, {
 $unwind: {
  path: '$cities15',
  preserveNullAndEmptyArrays: false
 }
}, {
 $project: {
  _id: {
   $concat: [
    {
     $replaceAll: {
      input: '$cities15.country',
      find: '/',
      replacement: '%2F'
     }
    },
    ' - ',
    {
     $replaceAll: {
      input: '$cities15.city_ascii',
      find: '/',
      replacement: '%2F'
     }
    }
   ]
  },
  position: [
   '$cities15.lng',
   '$cities15.lat'
  ],
  country: '$cities15.country'
 }
}, {
 $out: 'cities'
}])


2nd Pipeline--

first = { $sample: { size: 200} }
second = { $group: { _id: null, planes : { $push : { currentLocation : "$position" }}}}
unwind = { $unwind : {path: "$planes", includeArrayIndex: "id" }}
format = {$project : { _id : {$concat : ["CARGO",{$toString:"$id"}]},
currentLocation: "$planes.currentLocation", heading:{$literal:0}, route: []}}
asplanes = { $out: "planes"}
db.cities.aggregate([firstN,second,unwind,format,asplanes])
