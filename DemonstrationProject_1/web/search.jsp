<%-- 
    Document   : search
    Created on : Feb 25, 2025, 3:55:41 PM
    Author     : Acer
--%>

<%@page import="se1836.registration.RegistrationDTO"%>
<%@page import="java.util.List"%>
<%@page contentType="text/html" pageEncoding="UTF-8"%>
<!DOCTYPE html>
<html>
    <head>
        <title>Search page</title>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
    </head>
    <body>
        <h1>Search page!</h1>
        <form action="MainController">
            Search value: <input type="text" name="txtSearchValue" value="" /><br>
            <input type="submit" name="btAction" value="Search" /> 
        </form>
        <br/>
        
        <%
            String searchValue = request.getParameter("txtSearchValue");
            
            List<RegistrationDTO> result = (List<RegistrationDTO>) request.getAttribute("SEARCHRESULT");

            if (searchValue != null) {
                if (result != null) {
                    %>   
                    <table border="1">
                        <thead>
                            <tr>
                                <th>No.</th> 
                                <th>Username</th>
                                <th>Password</th>
                                <th>Lastname</th>
                                <th>Role</th>
                                <th>Delete</th>
                                <th>Update</th>
                            </tr>
                        </thead>
                        <tbody>
                            <% 
                                int count = 0;
                                for (RegistrationDTO dto : result) {
                                String urlRewriting = "MainController?btAction=Del&pk=" + dto.getUsername() + "&lastSearchValue=" + request.getParameter("txtSearchValue");
                                
                            %>
                        
                        
                            <tr>
                                <form action="MainController">
                                <td>
                                    <%= ++count%>
                                </td>
                                <td>
                                    <%= dto.getUsername()%>
                                    <input type="hidden" name="txtUsername" value="<%= dto.getUsername()%>">
                                </td>
                                <td>
                                    <input type="text" name="txtPassword" value="<%= dto.getPassword()%>" /> 
                                </td>
                                <td>
                                    <input type="text" name="txtLastname" value="<%=dto.getLastname()%>" />
                                </td>
                                <td>
                                    <%
                                    if(dto.isRoles()) {
                                    %>
                                        <input type="checkbox" name="chkAdmin" value="ADMIN" checked="checked"/>
                                    <% } else {

                                    %>
                                        <input type="checkbox" name="chkAdmin" value="ADMIN"/>
                                    <% }  

                                    %>
                                    
                                </td>
                                <td>
                                    <a href="<%=urlRewriting%>">Delete</a>
                                </td>
                                <td>
                                    <input type="submit" value="Update" name="btAction" />
                                    <input type="hidden" name="lastSearchValue" value="<%= request.getParameter("txtSearchValue")%>">
                                </td>
                                </form>
                            </tr>
                            <%
                                }
                            %>
                        
                        </tbody>
                    </table>
                    
            <%
                } else {
            %>
                <h2 style="color: red">No record is matched!!!</h2>
        <%
                }
            }
        %>
    </body>
</html>
