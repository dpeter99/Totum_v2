import {LiveData} from "@/lib/live-data/LiveData.ts";
import {ShoppingList} from "@/model/ShoppingList.ts";
import {ShoppingListApi, ShoppingListCreationDto} from "@/api";



export class ShoppingListService {
  
  public shoppingLists = new LiveData<ShoppingList[]>([], async ()=>{
    let data = await ShoppingListApi.getShoppingLists();
    
    this.shoppingLists.setValue(data.data!);
  });
  
  public async createShoppingList (data: ShoppingListCreationDto) {
    let res = await ShoppingListApi.createShoppingList({body:data});
    if(res.response.ok && res.data) {
      let newItem = ShoppingList.fromDto(res.data);
      this.shoppingLists.transition((l)=>[...l, newItem]);
    }
  }
  
  
}