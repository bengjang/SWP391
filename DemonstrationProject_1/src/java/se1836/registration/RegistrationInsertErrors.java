/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package se1836.registration;

import java.io.Serializable;

/**
 *
 * @author Acer
 */
public class RegistrationInsertErrors implements Serializable{
    private String usernameLengthErr;
    private String passwordLengthErr;
    private String confirmPasswordNotMatch;
    private String fullNameLengthErr;
    private String usernameIsExisted;

    public RegistrationInsertErrors() {
    }

    public RegistrationInsertErrors(String usernameLengthErr, String passwordLengthErr, String confirmPasswordNotMatch, String fullNameLengthErr, String usernameIsExisted) {
        this.usernameLengthErr = usernameLengthErr;
        this.passwordLengthErr = passwordLengthErr;
        this.confirmPasswordNotMatch = confirmPasswordNotMatch;
        this.fullNameLengthErr = fullNameLengthErr;
        this.usernameIsExisted = usernameIsExisted;
    }

    public String getUsernameLengthErr() {
        return usernameLengthErr;
    }

    public void setUsernameLengthErr(String usernameLengthErr) {
        this.usernameLengthErr = usernameLengthErr;
    }

    public String getPasswordLengthErr() {
        return passwordLengthErr;
    }

    public void setPasswordLengthErr(String passwordLengthErr) {
        this.passwordLengthErr = passwordLengthErr;
    }

    public String getConfirmPasswordNotMatch() {
        return confirmPasswordNotMatch;
    }

    public void setConfirmPasswordNotMatch(String confirmPasswordNotMatch) {
        this.confirmPasswordNotMatch = confirmPasswordNotMatch;
    }

    public String getFullNameLengthErr() {
        return fullNameLengthErr;
    }

    public void setFullNameLengthErr(String fullNameLengthErr) {
        this.fullNameLengthErr = fullNameLengthErr;
    }

    public String getUsernameIsExisted() {
        return usernameIsExisted;
    }

    public void setUsernameIsExisted(String usernameIsExisted) {
        this.usernameIsExisted = usernameIsExisted;
    }
    
    
    
}
