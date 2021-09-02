

//https://javascript.info/fetch
//fetchs data from url.
//returns data in JSON format.
async function fetchData(url){
    let response = await fetch(url,{
      method: 'GET'
    });
    if(response.ok){
      let result = await response.json();
      return result;
    }
    else{
      alert("HTTP-Error: " + response.status);
    }
  }