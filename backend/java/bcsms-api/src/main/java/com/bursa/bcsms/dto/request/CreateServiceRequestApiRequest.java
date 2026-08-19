package com.bursa.bcsms.dto.request;

import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Size;
import java.util.UUID;

public class CreateServiceRequestApiRequest {

    @NotBlank(message = "Title is required")
    @Size(max = 200, message = "Title cannot exceed 200 characters")
    private String title;

    @NotNull(message = "CategoryId is required")
    private UUID categoryId;

    private String description;
    private Double latitude;
    private Double longitude;
    private String addressText;

    public CreateServiceRequestApiRequest() {
    }

    public CreateServiceRequestApiRequest(String title, UUID categoryId, String description, Double latitude, Double longitude, String addressText) {
        this.title = title;
        this.categoryId = categoryId;
        this.description = description;
        this.latitude = latitude;
        this.longitude = longitude;
        this.addressText = addressText;
    }

    public String getTitle() { return title; }
    public void setTitle(String title) { this.title = title; }
    public UUID getCategoryId() { return categoryId; }
    public void setCategoryId(UUID categoryId) { this.categoryId = categoryId; }
    public String getDescription() { return description; }
    public void setDescription(String description) { this.description = description; }
    public Double getLatitude() { return latitude; }
    public void setLatitude(Double latitude) { this.latitude = latitude; }
    public Double getLongitude() { return longitude; }
    public void setLongitude(Double longitude) { this.longitude = longitude; }
    public String getAddressText() { return addressText; }
    public void setAddressText(String addressText) { this.addressText = addressText; }
}
