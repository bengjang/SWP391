/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/JSP_Servlet/Servlet.java to edit this template
 */
package se1836.controllers;

import jakarta.servlet.RequestDispatcher;
import java.io.IOException;
import java.io.PrintWriter;
import jakarta.servlet.ServletException;
import jakarta.servlet.annotation.WebServlet;
import jakarta.servlet.http.HttpServlet;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import java.sql.SQLException;
import se1836.registration.RegistrationDAO;
import se1836.registration.RegistrationInsertErrors;

/**
 *
 * @author Acer
 */
@WebServlet(name = "RegisterController", urlPatterns = {"/RegisterController"})
public class RegisterController extends HttpServlet {

    private final String REGISTERPAGE = "createnewaccount.jsp";
    private final String LOGINPAGE = "login.html";

    protected void processRequest(HttpServletRequest request, HttpServletResponse response)
            throws ServletException, IOException {
        response.setContentType("text/html;charset=UTF-8");
        try (PrintWriter out = response.getWriter()) {
            String url = REGISTERPAGE;
            String username = request.getParameter("txtUsername");
            String password = request.getParameter("txtPassword");
            String confirm = request.getParameter("txtConfirm");
            String fullName = request.getParameter("txtFullname");

            RegistrationInsertErrors errors = new RegistrationInsertErrors();
            boolean bError = false;
            try {
                if (username.trim().length() < 6 || username.trim().length() > 15) {
                    errors.setUsernameLengthErr("* username must be (6-15) chars *");
                    bError = true;
                }

                if (password.trim().length() < 6 || password.trim().length() > 15) {
                    errors.setPasswordLengthErr("* password must be (6-20) chars *");
                    bError = true;
                }

                if (!confirm.trim().equals(password.trim())) {
                    errors.setConfirmPasswordNotMatch("* confirm password not matched! *");
                    bError = true;
                }

                if (fullName.trim().length() < 2 || fullName.trim().length() > 50) {
                    errors.setFullNameLengthErr("* fullname must be (2-50) chars *");
                    bError = true;
                }

                if (bError == true) {
                    request.setAttribute("INSERTERROR", errors);
                } else {
                    RegistrationDAO dao = new RegistrationDAO();
                    boolean result = dao.insertRecord(username, password, fullName, false);

                    if (result) {
                        url = LOGINPAGE;
                    }
                }

            } catch (SQLException e) {
                errors.setUsernameIsExisted("* username is existed! *");
                request.setAttribute("INSERTERROR", errors);
            } finally {
                RequestDispatcher rd = request.getRequestDispatcher(url);
                rd.forward(request, response);
            }
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
