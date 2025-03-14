<%-- 
    Document   : viewCart
    Created on : Feb 28, 2025, 3:59:09 PM
    Author     : Acer
--%>

<%@page import="cart.CartObj"%>
<%@page import="java.util.Map"%>
<%@page contentType="text/html" pageEncoding="UTF-8"%>
<!DOCTYPE html>
<html>
    <head>
        <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
        <title>View cart Page</title>
    </head>
    <body>
        <h1>View Your Cart: </h1>
        
        <%
            if (session != null) {
                    CartObj cart = (CartObj)session.getAttribute("CART");
                    if (cart != null) {
                        if (cart.getItems() != null) {
                        %>
                        <table border="1">
                            <thead>
                                <tr>
                                    <th>No</th>
                                    <th>Title</th>
                                    <th>Quantity</th>
                                    <th>Action</th>
                                </tr>
                            </thead>
                            <tbody>
                            <form action="MainController">
                                <% 
                                    int count = 0;
                                    Map<String, Integer> items = cart.getItems();
                                    for (Map.Entry item : items.entrySet()) {
                                %>
                                    <tr>
                                    <td>
                                        <%=++count%>
                                    </td>
                                    <td>
                                        <%=item.getKey()%>
                                    </td>
                                    <td>
                                        <%=item.getValue()%>
                                    </td>
                                    <td>
                                        <input type="checkbox" name="chkTitle" value="<%= item.getKey()%>">
                                    </td>
                                </tr>
                                <%
                                    }
                                %>
                               
                                <tr>
                                    <td colspan="3">
                                        <a href="bookStore.html">Add more item to cart</a>
                                    </td>
                                    <td>
                                        <input type="submit" value="Remove Item" name="btAction">
                                    </td>
                                </tr>
                            </form> 
                            </tbody>
                        </table>
                        
                    <%
                        }
                    } else {
                    %>
                    <h2>Your cart is empty!</h2>
                    <%
                    }
                }
        %>
    </body>
</html>
