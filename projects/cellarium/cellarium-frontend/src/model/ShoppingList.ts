import {Brand} from "../utils";


export type ShoppingListID = Brand<string, 'ShoppingListId'>
export type ShoppingList = {
    id: ShoppingListID,
    name: string,
}