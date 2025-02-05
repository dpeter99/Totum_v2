import {ShoppingListItem} from "@/model/ShoppingListItem.ts";


export class ShoppingListItemService {
    
  public async getAllItems(){
      const res = await fetch('https://localhost:5002/api/shopping-list/item')
      return await res.json() as unknown as ShoppingListItem[];
  }

  public async addItem(param: { title: string }) {
    const res = await fetch('https://localhost:5002/api/shopping-list/item', {
      method: 'POST',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(param),
    })
    return await res.json(); 
  }
}