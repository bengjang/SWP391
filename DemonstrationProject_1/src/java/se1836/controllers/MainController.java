/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package se1836.controllers;

import java.io.IOException;
import java.io.PrintWriter;
import java.sql.SQLException;
import jakarta.servlet.RequestDispatcher;
import jakarta.servlet.ServletException;
import jakarta.servlet.http.HttpServlet;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import se1836.registration.RegistrationDAO;

/**
 *
 * @author Truong
 */ 
public class MainController extends HttpServlet {
    private final String LOGINCONTROLLER = "LoginController";
    private final String SEARCHCONTROLLER = "SearchController";
    private final String DELETECONTROLLER = "DeleteController";
    private final String UPDATECONTROLLER = "UpdateController";
    private final String ADDITEMCONTROLLER = "AddItemController";
    private final String VIEWCARTPAGE = "viewCart.jsp";
    private final String REMOVEITEMCONTROLLER = "RemoveItemController";
    private final String REGISTERCONTROLLER = "RegisterController";
    /**
     * Processes requests for both HTTP <code>GET</code> and <code>POST</code>
     * methods.
     *
     * @param request servlet request
     * @param response servlet response
     * @throws ServletException if a servlet-specific error occurs
     * @throws IOException if an I/O error occurs
     */
    protected void processRequest(HttpServletRequest request, HttpServletResponse response)
            throws ServletException, IOException {
        response.setContentType("text/html;charset=UTF-8");
        try (PrintWriter out = response.getWriter()) {
            /* TODO output your page here. You may use following sample code. */
            String action = request.getParameter("btAction");
            String url = "";
            if(action.equals("Login")){
                url = LOGINCONTROLLER;
            }else if(action.equals("Search")){
                url = SEARCHCONTROLLER;
            } else if (action.equals("Del")) {
                url = DELETECONTROLLER;
            } else if (action.equals("Update")) {
                url= UPDATECONTROLLER;
            } else if (action.equals("Add Item To Cart")) {
                url = ADDITEMCONTROLLER;
            } else if (action.equals("View Your Cart")){
                url = VIEWCARTPAGE;
            } else if(action.equals("Add Item To Cart")){
                url = ADDITEMCONTROLLER;
            }else if(action.equals("View Your Cart")){
                url = VIEWCARTPAGE;
            } else if (action.equals("Remove Item")) {
                url = REMOVEITEMCONTROLLER;
            } else if (action.equals("Register")) {
                url = REGISTERCONTROLLER;
            }
            RequestDispatcher rd  = request.getRequestDispatcher(url);
            rd.forward(request, response);          
        }
    }

    // <editor-fold defaultstate="collapsed" desc="HttpServlet methods. Click on the + sign on the left to edit the code.">
    /**
     * Handles the HTTP <code>GET</code> method.
     *
     * @param request servlet request
     * @param response servlet response
     * @throws ServletException if a servlet-specific error occurs
     * @throws IOException if an I/O error occurs
     */
    @Override
    protected void doGet(HttpServletRequest request, HttpServletResponse response)
            throws ServletException, IOException {
        processRequest(request, response);
    }

    /**
     * Handles the HTTP <code>POST</code> method.
     *
     * @param request servlet request
     * @param response servlet response
     * @throws ServletException if a servlet-specific error occurs
     * @throws IOException if an I/O error occurs
     */
    @Override
    protected void doPost(HttpServletRequest request, HttpServletResponse response)
            throws ServletException, IOException {
        processRequest(request, response);
    }

    /**
     * Returns a short description of the servlet.
     *
     * @return a String containing servlet description
     */
    @Override
    public String getServletInfo() {
        return "Short description";
    }// </editor-fold>

}
