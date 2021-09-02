import {fetchData} from '../tools/Fetch';


export async function fetchMethods(defaultValues={}, url){
    try{
        var result= await fetchData(url);
        return result;
    }
    catch{
        console.log("Failed to fetch property methods. Reload page to try again or continue with default.");
    }
    
    return defaultValues;
}

