package com.bursa.bcsms.dto.request;

public class WorkflowNoteApiRequest {
    private String note;

    public WorkflowNoteApiRequest() {
    }

    public WorkflowNoteApiRequest(String note) {
        this.note = note;
    }

    public String getNote() { return note; }
    public void setNote(String note) { this.note = note; }
}
