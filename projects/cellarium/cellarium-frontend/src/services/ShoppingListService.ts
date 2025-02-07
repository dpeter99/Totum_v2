import {LiveData} from "@/lib/live-data/LiveData.ts";
import {ShoppingList} from "@/model/ShoppingList.ts";
import {ShoppingListApi} from "@/api";



export class ShoppingListService {
  
  public shoppingLists = new LiveData<ShoppingList[]>([], async ()=>{
    let data = await ShoppingListApi.getShoppingLists();
    
    this.shoppingLists.setValue(data.data!);
  });
  
  
  
}