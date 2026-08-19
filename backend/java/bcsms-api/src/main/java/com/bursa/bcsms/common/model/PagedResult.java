package com.bursa.bcsms.common.model;

import java.util.List;

public class PagedResult<T> {
    private List<T> items;
    private int pageNumber;
    private int pageSize;
    private long totalItems;
    private int totalPages;
    private boolean hasNextPage;
    private boolean hasPreviousPage;

    public PagedResult() {
    }

    public PagedResult(List<T> items, int pageNumber, int pageSize, long totalItems) {
        this.items = items;
        this.pageNumber = pageNumber;
        this.pageSize = pageSize;
        this.totalItems = totalItems;
        this.totalPages = pageSize > 0 ? (int) Math.ceil((double) totalItems / pageSize) : 0;
        this.hasNextPage = pageNumber < totalPages;
        this.hasPreviousPage = pageNumber > 1;
    }

    public List<T> getItems() { return items; }
    public int getPageNumber() { return pageNumber; }
    public int getPageSize() { return pageSize; }
    public long getTotalItems() { return totalItems; }
    public int getTotalPages() { return totalPages; }
    public boolean isHasNextPage() { return hasNextPage; }
    public boolean isHasPreviousPage() { return hasPreviousPage; }
}
