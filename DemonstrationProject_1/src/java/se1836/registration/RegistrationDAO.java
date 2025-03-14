/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package se1836.registration;

import java.io.Serializable;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.List;
import se1836.database.DBUtils;

/**
 *
 * @author Truong
 */
public class RegistrationDAO implements Serializable {

    DBUtils db = new DBUtils();

    public boolean checkLogin(String username, String password) throws SQLException {
        Connection con = null;
        PreparedStatement stm = null;
        ResultSet rs = null;

        try {
            con = db.makeConnection();
            if (con != null) {
                String sql = "Select * From"
                        + " tblRegistration Where username = ? and password = ?";
                stm = con.prepareStatement(sql);
                stm.setString(1, username);
                stm.setString(2, password);     
                rs = stm.executeQuery();
                if (rs.next()) {
                    return true;
                }
            }

        } catch (SQLException ex) {
            ex.printStackTrace();
        } finally {
            if (rs != null) {
                rs.close();
            }
            if (stm != null) {
                stm.close();
            }
            if (con != null) {
                con.close();
            }
        }
        return false;
    }
    private List<RegistrationDTO> listAccounts;

    public List<RegistrationDTO> getListAccounts() {
        return listAccounts;
    }

    public void searchByLastname(String searchValue) throws SQLException {
        Connection con = null;
        PreparedStatement stm = null;
        ResultSet rs = null;
        try {
            con = db.makeConnection();
            if (con != null) {
                String sql = "Select * From tblRegistration Where lastname like ?";
                stm = con.prepareStatement(sql);
                stm.setString(1, '%' + searchValue + "%");
                rs = stm.executeQuery();
                while (rs.next()) {
                    String username = rs.getString("username");
                    String password = rs.getString("password");
                    String lastname = rs.getString("lastname");
                    boolean role = rs.getBoolean("isAdmin");
                    RegistrationDTO dto = new RegistrationDTO(username, password, lastname, role);
                    if (listAccounts == null) {
                        listAccounts = new ArrayList<>();
                    }
                    listAccounts.add(dto);
                }
            }
        } catch (SQLException e) {
            e.printStackTrace();
        } finally {
            if (rs != null) {
                rs.close();
            }
            if (stm != null) {
                stm.close();
            }
            if (con != null) {
                con.close();
            }
        }
    }

    public boolean deleteRecord(String pk) throws SQLException {
        Connection con = null;
        PreparedStatement stm = null;
        
        try {
            con = db.makeConnection();
            if (con != null ) {
                String sql = "Delete from tblRegistration Where username = ?";
                stm = con.prepareStatement(sql);
                stm.setString(1, pk);
                int row = stm.executeUpdate();
                if (row > 0) {
                    return true;
                }
            }
        } catch (SQLException e) {
        } finally {
            if (stm != null) {
                stm.close();
            }
            if (con != null) {
                con.close();
            }
        } 
        
        return false;
    }
    
    public boolean updateRecord(String password, String lastname, boolean role, String username) throws SQLException {
        
        Connection con = null;
        PreparedStatement stm = null;
        try {
            con = db.makeConnection();
            if (con != null) {
                String sql = "Update tblRegistration Set password = ?, lastname = ?,"
                        + " isAdmin = ? Where username = ?";
                stm = con.prepareStatement(sql);
                stm.setString(1, password);
                stm.setString(2, lastname);
                stm.setBoolean(3, role);
                stm.setString(4, username);
                int row = stm.executeUpdate();
                
                if (row > 0) {
                    return true;
                }
            }

        } finally {
            if (stm != null) {
                stm.close();
            }
            if (con != null) {
                con.close();
            }
        }
        
        return false;
    }
    
    public boolean insertRecord(String username, String password, String fullname, boolean role) throws SQLException {
        Connection con = null;
        PreparedStatement stm = null;
        
        try {
            con = db.makeConnection();
            if (con != null) {
                String sql = "Insert into tblRegistration (username, password, lastname, isAdmin) values (?, ?, ?, ?)";
                stm = con.prepareStatement(sql);
                stm.setString(1, username);
                stm.setString(2, password);
                stm.setString(3, fullname);
                stm.setBoolean(4, role);
                
                int row = stm.executeUpdate();
                if (row > 0) {
                    return true;
                }
            }
        } catch (SQLException e) {
        } finally {
            if (stm != null) {
                stm.close();
            }
            if (con != null) {
                con.close();
            }
        }
        
        return false;
    }
}
