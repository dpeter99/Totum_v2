import {Brand} from "../utils";
import {ShoppingListWithItemsDto} from "@/api";


export type ShoppingListID = Brand<string, 'ShoppingListId'>
export class ShoppingList {
    id: ShoppingListID;
    name: string;
    
    constructor(id: ShoppingListID, name: string) {
        this.id = id;
        this.name = name;
    }

    static fromDto(dto: ShoppingListWithItemsDto): ShoppingList {
        return new ShoppingList(
            dto.id,
            dto.name,
        );
    }
}

