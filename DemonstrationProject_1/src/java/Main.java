
import java.sql.Connection;
import se1836.database.DBUtils;

/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */

/**
 *
 * @author Truong
 */
public class Main {
    public static void main(String[] args) {
        DBUtils db = new DBUtils();
        Connection con = db.makeConnection();
        if(con!=null){
            System.out.println("Connected ");
        }else{
            System.out.println("Not connect try again");
        }
    }
}
