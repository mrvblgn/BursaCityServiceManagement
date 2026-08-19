package com.bursa.bcsms.domain.valueobject;

import jakarta.persistence.Column;
import jakarta.persistence.Embeddable;
import java.util.Objects;

@Embeddable
public class Location {

    @Column(name = "latitude")
    private Double latitude;

    @Column(name = "longitude")
    private Double longitude;

    @Column(name = "address_text", columnDefinition = "TEXT")
    private String addressText;

    protected Location() {
    }

    public Location(Double latitude, Double longitude, String addressText) {
        this.latitude = latitude;
        this.longitude = longitude;
        this.addressText = addressText != null ? addressText.trim() : null;
    }

    public Double getLatitude() {
        return latitude;
    }

    public Double getLongitude() {
        return longitude;
    }

    public String getAddressText() {
        return addressText;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        Location location = (Location) o;
        return Objects.equals(latitude, location.latitude) &&
                Objects.equals(longitude, location.longitude) &&
                Objects.equals(addressText, location.addressText);
    }

    @Override
    public int hashCode() {
        return Objects.hash(latitude, longitude, addressText);
    }
}
