<%-- 
    Document   : createnewaccount
    Created on : Mar 7, 2025, 4:21:01 PM
    Author     : Acer
--%>

<%@page import="se1836.registration.RegistrationInsertErrors"%>
<%@page contentType="text/html" pageEncoding="UTF-8"%>
<!DOCTYPE html>
<html>
    <head>
        <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
        <title>JSP Page</title>
    </head>
    <body>
        <h1>Create New Account</h1>

        <form action="MainController">
            Username(*): <input type="text" name="txtUsername" value="<%= request.getParameter("txtUsername")%>"> (6-15 chars) 
            <%
                RegistrationInsertErrors errors = (RegistrationInsertErrors) request.getAttribute("INSERTERROR");
                if (errors != null) {
                    if (errors.getUsernameLengthErr() != null) {
            %>
            <font color="red"><%=errors.getUsernameLengthErr()%> </font>
            <%
                    }

                }
            %>
            
            <%
                if (errors != null) {
                    if (errors.getUsernameIsExisted()!= null) {
            %>
            <font color="red"><%=errors.getUsernameIsExisted()%> </font>
            <%
                    }

                }
            %>
            <br/>
            Password(*): <input type="password" name="txtPassword" value=""> (6-20 chars)
            <%
                if (errors != null) {
                    if (errors.getPasswordLengthErr() != null) {
            %>
            <font color="red"><%= errors.getPasswordLengthErr()%> </font>
            <%
                    }
                }
            %>
            <br/>
            Confirm password(*): <input type="password" name="txtConfirm" value="">
            <%
                if (errors != null) {
                    if (errors.getConfirmPasswordNotMatch() != null) {
            %>
            <font color="red"><%= errors.getConfirmPasswordNotMatch()%> </font>
            <%
                    }
                }
            %>
            <br/>
            Fullname(*): <input type="text" name="txtFullname" value="<%= request.getParameter("txtFullname")%>"> (2-50 chars) 
            <%
                if (errors != null) {
                    if (errors.getFullNameLengthErr() != null) {
            %>
            <font color="red"><%= errors.getFullNameLengthErr()%> </font>
            <%
                    }
                }
            %>


            <br/>
            <input type="submit" value="Register" name="btAction"> 
            <input type="reset" value="Reset">
        </form>
    </body>
</html>
