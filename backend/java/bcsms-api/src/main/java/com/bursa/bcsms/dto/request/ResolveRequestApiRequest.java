package com.bursa.bcsms.dto.request;

public class ResolveRequestApiRequest {
    private String note;

    public ResolveRequestApiRequest() {
    }

    public ResolveRequestApiRequest(String note) {
        this.note = note;
    }

    public String getNote() { return note; }
    public void setNote(String note) { this.note = note; }
}
