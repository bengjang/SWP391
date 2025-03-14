/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package cart;

import java.io.Serializable;
import java.util.HashMap;
import java.util.Map;

/**
 *
 * @author Acer
 */
public class CartObj implements Serializable{
    private String customerID;
    private Map<String, Integer> items;

    public String getCustomerID() {
        return customerID;
    }

    public void setCustomerID(String customerID) {
        this.customerID = customerID;
    }

    public Map<String, Integer> getItems() {
        return items;
    }

    public void setItems(Map<String, Integer> items) {
        this.items = items;
    }
    
    public void addItemToCart(String title) {
        if (this.items == null) {
            this.items = new HashMap<String, Integer>();
            
        }
        
        int quantity = 1;
        if (this.items.containsKey(title)) {
            quantity = this.items.get(title) + 1;
        }
        this.items.put(title, quantity);
    }
    
    public void removeItemFromCart(String tiltle) {
        if (this.items == null) {
            return;
        } 
        
        if (this.items.containsKey(tiltle)) {
            this.items.remove(tiltle);
            if (this.items.isEmpty()) {
                this.items = null;
            }
        }
    }
    
}
